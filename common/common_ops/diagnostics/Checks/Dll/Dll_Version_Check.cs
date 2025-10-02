using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Dll
{
    /// <summary>
    /// Class description. Repair description
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: what will be returned and which postfixes</para>
    /// 
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, WARNING, REPAIR, ERROR </para>
    /// </summary>
    public class Dll_Version_Check : ICheck
    {
        private readonly FileContextBuilder _fileContextBuilder = new FileContextBuilder();

        private readonly bool _repair;
        private readonly string[] REQUIRED_DLLS;
        private readonly ICheck _dllLocationCheck;
        private readonly IDirectorySystem _directorySystem;
        private string _localDllFolder;
        private readonly string _sourceDllFolder;


        /// <summary>
        /// <inheritdoc cref="Dll_Version_Check"/>
        /// </summary>
        public Dll_Version_Check(ICheck dllLocationCheck, IDirectorySystem directorySystem, string localDllFolder = null, bool doRepair = false)
        {
            _dllLocationCheck = dllLocationCheck;
            _directorySystem = directorySystem;
            _localDllFolder = localDllFolder;
            _repair = doRepair;

            var aditionalDlls = new string[]
            {
                "AndOnlineSyncCliPanel",
                "AndOnlineSyncCmn",
                "zxing",
            };

            REQUIRED_DLLS = BiroNextConstants.RequiredBirokratDlls.Union(aditionalDlls).ToArray();
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            //1. find all localFiles full path.
            //2. if version matches look at creation date?
            var DD = TextConstants.DELIMITER;
            List<string> additionalInfo = new List<string>();

            if (string.IsNullOrEmpty(_localDllFolder))
            {
                var location = await _dllLocationCheck.Run();
                _localDllFolder = location.AdditionalInfo.FirstOrDefault();
                if (string.IsNullOrEmpty(_localDllFolder))
                    return new ResultRecord(false, GetType().Name, "Could not find Dll root folder at: '" + _localDllFolder + "'");
            }

            var localRecords = BuildFileRecords(_localDllFolder).ToArray();
            var sourceRecords = BuildFileRecords(BiroLocationConstants.DefaultDLLOrigin).ToArray();

            foreach (var record in localRecords)
            {
                foreach (var testRecord in sourceRecords)
                {
                    if (record.Name.Equals(testRecord.Name))
                    {
                        if (record.ModifiedTime == testRecord.ModifiedTime)
                            additionalInfo.Add($"{testRecord.Name}{DD}{DD}{TextConstants.POSTFIX_OK}");
                        else
                            additionalInfo.Add($"{testRecord.Name}{DD}Local file outdated{DD}{TextConstants.POSTFIX_ERROR}");

                        break;
                    }
                }
            }

            var missing = FindMissingDlls(localRecords).ToArray();
            additionalInfo.AddRange(missing);

            var result = true;
            if (localRecords.Length != sourceRecords.Length)
                result = false;
            if (!additionalInfo.All(x => x.EndsWith(TextConstants.POSTFIX_OK)))
                result = false;

            return new ResultRecord(result, GetType().Name, additionalInfo.ToArray());
        }

        private IEnumerable<SFileContext> BuildFileRecords(string location)
        {
            var files = FindRequestedFiles(location);
            foreach (var file in files)
            {
                if (File.Exists(file))
                    yield return _fileContextBuilder.GetFileData(file);
            }
        }

        private string[] FindRequestedFiles(string location)
        {
            var files = _directorySystem.GetFiles(location, "*.dll", SearchOption.AllDirectories);
            return FilterRequestedFiles(files).ToArray();
        }

        private IEnumerable<string> FilterRequestedFiles(string[] localFiles)
        {
            string Filter(string required, string[] local)
            {
                foreach (var loc in local)
                {
                    if (loc.EndsWith(required, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return loc;
                    }
                }
                return $"{required} {TextConstants.POSTFIX_WARNING}";
            }

            foreach (var required in REQUIRED_DLLS)
            {
                yield return Filter($"{required}.dll", localFiles);
            }
        }

        private IEnumerable<string> FindMissingDlls(SFileContext[] localRecords)
        {
            var DD = TextConstants.DELIMITER;

            foreach (var required in REQUIRED_DLLS)
            {
                if (!localRecords.Any(x => x.FullName.EndsWith($"{required}.dll")))
                    yield return $"{required}{DD}Missing{DD}{TextConstants.POSTFIX_ERROR}";
            }
        }
    }
}

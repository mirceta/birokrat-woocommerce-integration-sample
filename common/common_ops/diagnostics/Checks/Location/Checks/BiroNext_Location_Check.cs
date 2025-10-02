using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Location.Checks
{
    /// <summary>
    /// Looks for BiroNext Location folder. If location is provided in constructor it will perform check for said location first.
    /// If search will fail it will look for default next location (C:\Birokrat\NextGlobal\LATEST). If any of location checks will
    /// succeed it will do a simple check if all required folders are present and if runner_global.exe can be found.
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: BiroNext root folder.
    /// Will return empty string if next root location was not found or array of missing folders if next location was found but not all next folders are included </para>
    /// </summary>
    public class BiroNext_Location_Check : ICheck
    {
        private readonly ILocationHelper _locationHelper;
        private readonly IFileSystem _fileSystem;
        private readonly IDirectorySystem _directorySystem;
        private readonly string _location;

        /// <summary>
        /// <inheritdoc cref="BiroNext_Location_Check"/>
        /// </summary>
        public BiroNext_Location_Check(ILocationHelper locationHelper, IFileSystem fileSystem, IDirectorySystem directorySystem, string location = "")
        {
            _locationHelper = locationHelper;
            _fileSystem = fileSystem;
            _directorySystem = directorySystem;
            _location = location;
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

        public Task<ResultRecord> Work()
        {
            var folderCheck = _locationHelper.CheckIfFolderExists(_location, BiroLocationConstants.DefaultNextLocation);

            ResultRecord record;
            if (folderCheck.result)
            {
                var directories = _directorySystem.GetDirectoriesInfo(folderCheck.location);
                var allFoldersCheck = _locationHelper.AreAllRequiredFoldersPresent(directories, BiroLocationConstants.NextFolders);
                var runnerCheck = _fileSystem.Exists(Path.Combine(folderCheck.location, BiroLocationConstants.LocalRunnerGlobalPath));

                var result = allFoldersCheck.Result && runnerCheck;

                if (!result)
                    record = new ResultRecord(result, GetType().Name, allFoldersCheck.CheckInfo);
                else
                    record = new ResultRecord(result, GetType().Name, result ? folderCheck.location : string.Empty);
            }
            else
                record = new ResultRecord(false, GetType().Name, string.Empty);

            return Task.FromResult(record);
        }
    }
}

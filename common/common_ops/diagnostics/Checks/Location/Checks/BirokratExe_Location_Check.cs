using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Location.Checks
{
    /// <summary>
    /// Looks for Birokrat.Exe folder location. It will search for location provided in constructor first and if it fails it will look for
    /// default birokrat instalation location (C:\Birokrat). Result reurns true if birokrat folder exist and if contains Birokrat.exe
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: Location to Birokrat folder that contains Birokrat.exe.
    /// WIll return empty string if nothing was found</para>
    /// </summary>
    public class BirokratExe_Location_Check : ICheck
    {
        private readonly ILocationHelper _locationHelper;
        private readonly IFileSystem _fileExists;
        private readonly string _location;

        /// <summary>
        /// <inheritdoc cref="BirokratExe_Location_Check"/>
        /// </summary>
        public BirokratExe_Location_Check(ILocationHelper locationHelper, IFileSystem fileExists, string location = "")
        {
            _locationHelper = locationHelper;
            _fileExists = fileExists;
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

        private Task<ResultRecord> Work()
        {
            var folderCheck = _locationHelper.CheckIfFolderExists(_location, BiroLocationConstants.BirokratDefaultLocation);

            ResultRecord record;
            if (folderCheck.result)
            {
                var fileCheck = _fileExists.Exists(Path.Combine(folderCheck.location, BiroLocationConstants.BirokratExeFileName));
                var result = folderCheck.result && fileCheck;
                record = new ResultRecord(result, GetType().Name, result ? folderCheck.location : string.Empty);
            }
            else
                record = new ResultRecord(false, GetType().Name, string.Empty);

            return Task.FromResult(record);
        }
    }
}

using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Location.Checks
{
    /// <summary>
    /// Look for Birokrat.DLL folder. Will perform a search in default location. If location is provided in constructor parameter it will
    /// search that folder first. If folder is found it will check if Birokrat.DLL folder has Required Folders. If search was not successfull
    /// it will perform same search in default location (C:\Birokrat.DLL). Will return false is Birokrat.DLL folder is not found OR if
    /// Birokrat.DLL folder does not contain required folders. 
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: Birokrat.DLL folder.
    /// WIll return empty string if nothing was found.
    /// </summary>
    public class BirokratDll_Location_Check : ICheck
    {
        private string _location;
        private readonly ILocationHelper _locationHelper;
        private readonly IDirectorySystem _directorySystem;

        /// <summary>
        /// <inheritdoc cref="BirokratDll_Location_Check"/>
        /// </summary>
        public BirokratDll_Location_Check(ILocationHelper locationHelper, IDirectorySystem directorySystem, string location = "")
        {
            _location = location;
            _locationHelper = locationHelper;
            _directorySystem = directorySystem;
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
            var folderCheck = _locationHelper.CheckIfFolderExists(_location, BiroLocationConstants.BirokratDLL_DefaultFolder);

            ResultRecord record;
            if (folderCheck.result)
            {
                var directories = _directorySystem.GetDirectoriesInfo(folderCheck.location);
                var requiredFoldersCheck = _locationHelper.AreAllRequiredFoldersPresent(directories, BiroLocationConstants.BirokratDLL_RequredFolders);

                var result = requiredFoldersCheck.Result;

                if (!result)
                    record = new ResultRecord(result, GetType().Name, requiredFoldersCheck.CheckInfo);
                else
                    record = new ResultRecord(result, GetType().Name, folderCheck.location);
            }
            else
                record = new ResultRecord(false, GetType().Name, GetNotFoundMessage());

            return Task.FromResult(record);
        }

        private string GetNotFoundMessage()
        {
            return "Dll folder was not found in default location: '" + BiroLocationConstants.BirokratDLL_DefaultFolder + "'";
        }
    }
}

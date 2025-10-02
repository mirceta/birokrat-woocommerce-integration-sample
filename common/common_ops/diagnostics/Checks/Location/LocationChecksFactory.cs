using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;

namespace common_ops.diagnostics.Checks.Location
{
    public class LocationChecksFactory
    {
        /// <summary>
        /// Looks for Birokrat.Exe folder location. It will search for location provided in constructor first and if it fails it will look for
        /// default birokrat instalation location (C:\Birokrat). Result returns true if birokrat folder exist and if contains Birokrat.exe
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: Location to Birokrat folder that contains Birokrat.exe.
        /// WIll return empty string if nothing was found</para>
        /// </summary>
        public ICheck Build_BiroExeLocationCheck(string location = "")
        {
            return new BirokratExe_Location_Check(new LocationHelper(), new FileSystem(), location);
        }

        /// <summary>
        /// Look for Birokrat.DLL folder. Will perform a search in default location. If location is provided in constructor parameter it will
        /// search that folder first. If folder is found it will check if Birokrat.DLL folder has Required Folders. If search was not successfull
        /// it will perform same search in default location (C:\Birokrat.DLL). Will return false is Birokrat.DLL folder is not found OR if
        /// Birokrat.DLL folder does not contain required folders. 
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: Birokrat.DLL folder.
        /// WIll return empty string if nothing was found.
        /// </summary>
        public ICheck Build_BiroDllLocationCheck(string location = "")
        {
            return new BirokratDll_Location_Check(new LocationHelper(), new DirectorySystem(), location);
        }

        /// <summary>
        /// Looks for BiroNext Location folder. If location is provided in constructor it will perform check for said location first.
        /// If search will fail it will look for default next location (C:\Birokrat\NextGlobal\LATEST). If any of location checks will
        /// succeed it will do a simple check if all required folders are present and if runner_global.exe can be found.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: BiroNext root folder.
        /// Will return empty string if check was not successful ie <see cref="ResultRecord.Result"/> is false </para>
        /// </summary>
        public ICheck Build_BiroNextLocationCheck(string location = "")
        {
            return new BiroNext_Location_Check(new LocationHelper(), new FileSystem(), new DirectorySystem(), location);
        }

        /// <summary>
        /// Will check if sqlbirokrat has ReadAndWrite permission.
        /// Result is determined if permissions is true.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: path_to_location||result for permission check.
        /// categories are separated with <c>||</c>. Example:</para>
        /// <para>\\\\sqlbirokrat\\Andersen||true</para>
        /// </summary>
        ///
        public ICheck Build_SqlBirokratAccessAndPermissionCheck()
        {
            return new Sqlbirokrat_AccessAndPermission_Check(new LocationHelper());
        }

        /// <summary>
        /// Will check if sqlbirokrat has Read permission.
        /// Result is determined if permissions is true.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: path_to_location||result for permission check.
        /// categories are separated with <c>||</c>. Example:</para>
        /// <para>\\\\sqlbirokrat\\Andersen||true</para>
        /// </summary>
        ///
        public ICheck Build_SqlBirokrat2AccessAndPermissionCheck()
        {
            return new Sqlbirokrat2_AccessAndPermission_Check(new LocationHelper());
        }
    }
}

using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location;
using common_ops.diagnostics.Checks.Next.Checks;
using common_ops.diagnostics.Checks.Next.Utils;
using common_ops.Executors.Shell;
using common_ops.Executors.Sql;
using common_ops.FileHandler;

namespace common_ops.diagnostics.Checks.Next
{
    public class NextChecksFactory
    {
        /// <summary>
        /// Checks and optionally repairs the settings and secrets _files in the specified BiroNext root folder.
        /// Validates whether required appsettings and secrets _files exist and ensures their configurations match the expected SQL Server name.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:
        /// Validation results for settings _files (e.g., missing _files, incorrect configurations, repair logs...).
        /// Validation results for secrets _files (e.g., missing _files, incorrect configurations, repair logs...).
        /// If everything is good all entries will be ending wih 'OK'.</para>
        ///
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, WARNING, REPAIR, ERROR </para>
        /// 
        /// <para>Will return false if any of the following is true:
        /// 1. The specified root folder is not a valid BiroNext location.
        /// 2. Required settings or secrets _files are missing.
        /// 3. The repair option is not enabled AND Configurations do not match the expected values.</para>
        /// </summary>
        public BiroNext_Settings_CheckAndRepair Build_NextSettingsCheckAndRepair(string nextRootFolder, string sqlServerName, string credentialsSqlServerName, bool doRepair = false)
        {
            return new BiroNext_Settings_CheckAndRepair(
                new LocationChecksFactory().Build_BiroNextLocationCheck(nextRootFolder),
                new DirectorySystem(),
                new SqlUtils(),
                new JsonParser(),
                nextRootFolder,
                sqlServerName,
                credentialsSqlServerName,
                doRepair);
        }

        /// <summary>
        /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: If beta in biro_instance_pool is the same as default birokrat.exe
        /// it will only return 1 row since production and beta are the same. Otherwise it will return all birokrat.exes and display which one is set
        /// as beta
        /// (<see cref="TextConstants.DELIMITER"/>)
        /// </summary>
        public ICheck Build_BiroNext_BetaExe_Check(string baseNextDirectory)
        {
            return new BiroNext_BetaExe_Check(
                new DirectorySystem(),
                new PathSystem(),
                new SimpleSettingsLoader(new FileSystem()),
                baseNextDirectory);
        }

        /// <summary>
        /// By default Checks if the specified TCP ports (5000, 19000, 19001, 19002, 19005) are open. Optionaly you can provide ports as an argument in constructor.
        /// Executes a shell command to retrieve the status of the ports and determines if they are properly opened.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: [0] - requested ports, [1] requested ports that are properly opened.
        /// Will return false if the shell command fails or if all requeste ports are not opened.</para>
        /// </summary>
        public ICheck Build_BiroNext_OpenedPorts_Check(params int[] ports)
        {
            return new BiroNext_OpenedPorts_Check(new ShellExecutor(), ports);
        }

        public ICheck Build_BiroNext_Versoning_Check(string biroNextLocation)
        {
            return new BiroNext_Versioning_Check(new Birokrat_To_Bironext_VersionHandler(), biroNextLocation);
        }

        /// <summary>
        /// Will compare the file count between source and origin directory. It will only count folders that are required for Next to run (as defined
        /// in <see cref="BiroLocationConstants.NextFolders"/>) and appsettings.json in base folder. If both file counts matches result will be true.
        /// Class allows to exclude specific names or specifix extensions. In this example it is advised to exclude .txt _files (logs).
        /// </summary>
        /// <remarks>
        /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: file count for origin and source separated with ||
        /// (<see cref="TextConstants.DELIMITER"/>)
        /// </remarks>
        public ICheck Build_BiroNext_FileCountVerifier_Check(string sourceDirectory, string originDirectory, params string[] filesToExclude)
        {
            return new BiroNext_FileCountVerifier_Check(
                new DirectoryContentHandlerFactory().Build(null),
                sourceDirectory,
                originDirectory,
                filesToExclude);
        }
    }
}

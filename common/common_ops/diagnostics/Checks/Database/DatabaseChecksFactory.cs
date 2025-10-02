using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.Executors.Sql;

namespace common_ops.diagnostics.Checks.Database
{
    public class DatabaseChecksFactory
    {
        public BirokratQueryExecutor Build_BirokratQueryExecutor()
        {
            return new BirokratQueryExecutor(new DatabaseQueryExecutor());
        }

        /// <summary>
        /// Checks whether connection to SqlServer can be established. Will return false if connection cant be established. 
        /// For example when wrong sql server name is provided in connection string
        /// 
        /// <para><see cref="ResultRecord.AdditionalInfo"/>will always be empty. Only look at <see cref="ResultRecord.Result"/></para>
        /// </summary>
        public ICheck Build_SqlServer_Connection_Check(string connectionString)
        {
            return new SqlServer_Connection_Check(new SqlUtils(), connectionString);
        }

        /// <summary>
        /// Checks for core databases on server (application, biromaster and configuration)
        /// Results are determined based on whether any databases are found. Will return false if any of core databases is not present
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: databaseName or in case of an error databaseName||null. Separated with <c>||</c> </para>
        /// </summary>
        public ICheck Build_BirokratDatabase_CoreDatabases_Check(string connectionString)
        {
            return new BirokratDatabase_CoreDatabases_Check(
                Build_BirokratQueryExecutor(),
                connectionString);
        }

        /// <summary>
        /// Performs ProgramVersion check for the provided SqlServer. Results are determined based on whether ProgramVersion is retrieved from database.
        /// Will return false if no version is retrieved or if major version is not matching local Birokrat.exe version.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: [0] = DatabaseVersion, [1] = BirokratExeVersion, [2] = Major Version check, [3] = Minor Version check</para>
        /// </summary>
        public ICheck Build_BirokratDatabase_ProgramVersion_Check(string connectionString, string taxNumber, string birokratExeVersion, bool isSinhroAndBazure = false)
        {
            return new BirokratDatabase_ProgramVersion_Check(new DatabaseQueryExecutor(), connectionString, taxNumber, birokratExeVersion, isSinhroAndBazure);
        }

        /// <summary>
        /// Checks for yearcode databases on server. Will also check if -KRATEK and -SINHRO are present. Will return
        /// false if -KRATEK and -SINHRO or no databases with TaxNumber are present.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: databaseName. In case if -KRATEK or -SINHRO are not found: databaseName||null. Separated with <c>||</c>.
        /// Array will be empty if no databases for corresponding TaxNumber are found</para>
        /// </summary>

        public ICheck Build_BirokratDatabase_YearcodeDatabases_Check(string connectionString, string taxNumber)
        {
            return new BirokratDatabase_YearcodeDatabases_Check(
                Build_BirokratQueryExecutor(),
                connectionString,
                taxNumber);
        }

        /// <summary>
        /// Performs if TaxNumber is present in entityCompany in biromaster.
        /// Results are determined based on whether any TaxNumber is retrieved
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: Entity company name</para>
        /// </summary>
        public ICheck Build_Biromaster_IsTaxNumbersPresent_Check(string connectionString, string taxNumber)
        {
            return new Biromaster_IsTaxNumbersPresent_Check(
                new DatabaseQueryExecutor(),
                connectionString,
                taxNumber);
        }

        /// <summary>
        /// Performs APIKey check for the provided TaxNumber. Will return all api keys and corresponding users
        /// Results are determined based on whether any _apiKey is retrieved
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: APIKeys with corresponding user in format: _apiKey||User. Separated with <c>||</c> </para>
        /// </summary>
        public ICheck Build_BiroNext_ApiKeys_Check(string connectionString, string taxNumber)
        {
            return new BiroNext_ApiKeys_Check(new DatabaseQueryExecutor(), connectionString, taxNumber);
        }

        /// <summary>
        /// Performs SifreOperaterjev check for the provided TaxNumber. TaxNumber is ment to be 00000000 which is used by pinger. Will return true if there exists
        /// any operater that can login to bironext and operater have valid (not null) yearcode. 
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: operater operater||yearcode pairs, separated with <c>||</c>. If this yearcode's database
        /// is not present on the sql server, then this yearcode will be postfixed with <c>ERROR</c>. If repair option is chosen it will update the yearcode to latest
        /// one and will return repair result in 2nd row as 1||yearcode REPAIR
        /// </para>
        /// </summary>
        public ICheck Build_BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair(string connectionString, string taxNumber = "00000000")
        {
            var databaseQueryExecutor = new DatabaseQueryExecutor();

            return new BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair(
                new BirokratQueryExecutor(databaseQueryExecutor),
                databaseQueryExecutor,
                connectionString,
                taxNumber);
        }

        /// <summary>
        /// Performs SifreOperaterjev check for the provided TaxNumber. Will return true if there exists any operater that can login to bironext
        /// and all operaters have valid (not null) yearcodes
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: operater operater||yearcode pairs, separated with <c>||</c>. We have a row for each operater and their
        /// corresponding yearcode. If this yearcode's database is not present on the sql server, then this yearcode will be postfixed with <c>-ERROR</c>
        /// </para>
        /// <remarks>
        /// <para>Example of <see cref="ResultRecord.AdditionalInfo"/>:</para>
        /// <list type="bullet">
        /// <item><description>databases in SQL server</description></item>
        /// <item><description>biro00000000-I13</description></item>
        /// <item><description>biro00000000-J13</description></item>
        /// <item> <description>operater,yearcode:</description></item>
        /// <item><description>raco,I13</description></item>
        /// <item><description>kris,H18</description></item>
        /// <item><description><see cref="ResultRecord.AdditionalInfo"/></description></item>
        /// <item><description>[raco||I13],[kris||H18-ERROR]</description></item>
        /// </list>
        /// </remarks>
        /// </summary>
        public ICheck Build_BiroNext_SifreOperaterjev_Check(string connectionString, string taxNumber)
        {
            var databaseQueryExecutor = new DatabaseQueryExecutor();

            return new BiroNext_SifreOperaterjev_Check(
                new BirokratQueryExecutor(databaseQueryExecutor),
                databaseQueryExecutor,
                connectionString,
                taxNumber);
        }

        /// <summary> TODO !!!!!!!
        /// Checks for core databases on server (application, biromaster and configuration)
        /// Results are determined based on whether any databases are found. Will return false if any of core databases is not present
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: databaseName or in case of an error databaseName||null. Separated with <c>||</c> </para>
        /// </summary>
        public ICheck Build_BiroSinhro_Bazure1vsBazure5_Check()
        {
            return new BiroSinhro_Bazure1vsBazure5_Check(new DatabaseQueryExecutor());
        }

        /// <summary>
        /// Performs a collation check for the server and yearcode databases. 
        /// Results are determined based on whether the collation matches the specified default collation provided in constructor as argument.
        /// 
        /// <para>If the default collation is not met, results will return as database||collation, separated with <c>||</c>.
        /// The overall result will also be <c>true</c> if a yearcode database's collation is null but the main server's collation matches the `_defaultCollation`.</para>
        ///
        /// <remarks>
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>[0]: Main SQL Server collation.</description>
        /// </item>
        /// <item>
        /// <description>[1 and subsequent]: Distinct yearcode database collations.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// If additional info only contains one record with default collation all databases share the same collation
        /// </summary>
        public ICheck Build_SqlServer_Collation_Check(string connectionString, string taxNumber, string defaultCollation = "Slovenian")
        {
            var databaseQueryExecutor = new DatabaseQueryExecutor();

            return new SqlServer_Collation_Check(
               new BirokratQueryExecutor(databaseQueryExecutor),
               databaseQueryExecutor,
               connectionString,
               taxNumber,
               defaultCollation);
        }
    }
}

namespace common_ops.diagnostics.Checks.Database.Utils
{
    internal static class SqlQueries

    {
        internal static string GetDatabaseCollation(string databaseName)
        {
            return $"SELECT DATABASEPROPERTYEX('{databaseName}', 'Collation') AS Collation";
        }

        internal static string GetDefaultServerColation()
        {
            return "SELECT SERVERPROPERTY('Collation') AS ServerCollation;";
        }

        internal static string GetDatabasesNames()
        {
            return "SELECT name FROM sys.databases;";
        }

        internal static string GetYearcodeDatabaseNames(string taxNumber)
        {
            return $@"
                SELECT UpperName AS name
                FROM (
                    SELECT UPPER(name) AS UpperName
                    FROM sys.databases
                ) AS NormalizedNames
                WHERE 
                    UpperName NOT LIKE '%-KRATEK%' AND 
                    UpperName NOT LIKE '%-SINHRO%' AND
                    UpperName NOT LIKE '%TEMP%' AND
                    UpperName LIKE '%{taxNumber}%'";
        }

        internal static string GetProgramVersionFromKratek(string taxNumber)
        {
            return @"
               USE [biro" + taxNumber + @"-KRATEK];
               SELECT MAX(VerzijaPrograma) as Verzija From PoslovnaLeta;";
        }

        internal static string GetProgramVersionFomBiromaster(string taxNumber)
        {
            return @"
                USE biromaster;
                SELECT year, year_code, local_version, remote_version FROM entity_company_year cy
                INNER JOIN entity_company ek ON ek.pk_entity_company_id =  cy.fk_entity_company_id
                WHERE ek.tax_number = '" + taxNumber + @"'
                ORDER BY cy.year DESC;";
        }


        internal static string GetVerifyTaxNumber(string taxNumber)
        {
            return @"
                USE biromaster;
                SELECT [name] FROM entity_company 
                WHERE tax_number = '" + taxNumber + "';";
        }

        internal static string GetApiKeys(string davcna)
        {
            return @"
                USE [application];
                SELECT ak.[Value],ak.[UserName],ak.[Type] FROM ApiKeys ak
                INNER JOIN Companies co ON co.id = ak.CompanyId
                WHERE co.[NormalizedUserName] = '" + davcna + "';";
        }

        internal static string GetSifreOperaterjev(string taxNumber)
        {
            return @"
                USE [biro" + taxNumber + @"-KRATEK];
                SELECT Operater,OznakaLeta FROM SifreOperaterjev;";
        }

        internal static string UpdateSifraOperaterjaForPinger(string yearcode)
        {
            return $@"USE [biro00000000-KRATEK];
                UPDATE SifreOperaterjev SET OznakaLeta = '{yearcode}'
                WHERE Operater = '1';";
        }

        internal static string GetSifraOperaterjaAndYearcodeForPinger()
        {
            return $@"USE [biro00000000-KRATEK];
               SELECT Operater,OznakaLeta FROM SifreOperaterjev
               WHERE Operater = '1';";
        }
    }
}

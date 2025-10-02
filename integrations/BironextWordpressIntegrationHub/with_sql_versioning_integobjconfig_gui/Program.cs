using si.birokrat.next.common.database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace with_sql_versioning_integobjconfig_gui
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string connectionString = @"Server=localhost\MSSQLSERVER01;Database=integrations_db;Trusted_Connection=True;";
            Application.Run(new Versioning_IntegrationObjectConfigForm(
                            connectionString: connectionString,
                            nameOfIntegration: "PARTYPEK_WOOTOBIRO_PRODUCTION"));
        }
    }
}

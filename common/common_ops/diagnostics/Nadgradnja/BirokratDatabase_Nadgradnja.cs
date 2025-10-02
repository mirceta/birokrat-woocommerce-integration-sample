using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Nadgradnja
{
    /// <summary>
    /// THis will run script to perform Birokrat database Nadgradnja. DO NOT! do this with production databases of cusomers. This is ONLY
    /// for 00000000 database. In testing environment we can also use this for customer databases.
    /// With this step we resolve correct program version is stored in database. We also resolve that pinger will not fail
    /// on outdated databases.
    /// <para><see cref="ResultRecord.AdditionalInfo"/>will return true Result if everything is ok<see cref="ResultRecord.Result"/></para>
    /// </summary>
    public class BirokratDatabase_Nadgradnja : ICheck
    {
        private readonly string _sqlServerName;
        private readonly string _birokratLocation;
        private readonly string _vnasalec;
        private readonly string _poslovnoLeto;
        private readonly string _davcna;

        /// <summary>
        /// <inheritdoc cref="BirokratDatabase_Nadgradnja"/>
        /// </summary>
        public BirokratDatabase_Nadgradnja(string sqlServerName, string birokratLocation, string vnasalec, string poslovnoLeto, string davcna)
        {
            _sqlServerName = sqlServerName;
            _birokratLocation = birokratLocation;
            _vnasalec = vnasalec;
            _poslovnoLeto = poslovnoLeto;
            _davcna = davcna;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                var start = DateTime.Now;

                var executor = new ShellExecutor();
                var command = ShellExecutionMethod(_sqlServerName, _birokratLocation, _vnasalec, _poslovnoLeto, _davcna);
                await executor.ExecuteInBackgroundAsync(command);

                var duration = DateTime.Now - start;

                return new ResultRecord(duration.Seconds > 15, GetType().Name, string.Empty);
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private string ShellExecutionMethod(string sqlServerName, string birokratLocation, string vnasalec, string poslovnoLeto, string davcna)
        {
            var birokratExe = BiroLocationConstants.BirokratExeFileName;
            var birokratExeLocation = Path.Combine(birokratLocation, birokratExe);

            return $@"
                $nadgradnja = Start-Job -ScriptBlock {{
                    Start-Process ""{birokratExeLocation}"" '###UPGRADE###{vnasalec}###{poslovnoLeto}###{sqlServerName}###{davcna}###{birokratLocation}######' -Wait
                }}
                Wait-Job $nadgradnja
                $output = Receive-Job -Job $nadgradnja
                Remove-Job -Job $nadgradnja
                Write-Output $output";
        }
    }
}

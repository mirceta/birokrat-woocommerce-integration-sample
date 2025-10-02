using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Next.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Next.Checks
{
    /// <summary>
    /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: If beta in biro_instance_pool is the same as default birokrat.exe
    /// it will only return 1 row since production and beta are the same. Otherwise it will return all birokrat.exes and display which one is set
    /// as beta
    /// (<see cref="TextConstants.DELIMITER"/>)
    /// </summary>
    public class BiroNext_BetaExe_Check : ICheck
    {
        private readonly IDirectorySystem _directorySystem;
        private readonly IPathSystem _pathSystem;
        private readonly ISimpleSettingsLoader _settingsLoader;
        private readonly string _sourceDirectory;
        private readonly string BETA_KEY = "biro_instance_pool";

        /// <summary>
        /// <inheritdoc cref="BiroNext_BetaExe_Check"/>
        /// </summary>
        public BiroNext_BetaExe_Check(IDirectorySystem directorySystem, IPathSystem pathSystem, ISimpleSettingsLoader settingsLoader, string baseNextDirectory)
        {
            _directorySystem = directorySystem;
            _pathSystem = pathSystem;
            _settingsLoader = settingsLoader;
            _sourceDirectory = baseNextDirectory;
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
            var c = "\"birokrat_beta_name\": \"Birokrat.exe\"";

            var task = Task.Run(() =>
            {
                var betaExe = GetAndRepairInstancePoolSettings();

                var birokrats = _directorySystem.EnumerateFiles(BiroLocationConstants.BirokratDefaultLocation)
                    .Where(x => _pathSystem.GetFileName(x).ToLower()
                    .StartsWith("birokrat", StringComparison.OrdinalIgnoreCase))
                    .Where(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (birokrats.Any(x => x.EndsWith(BiroLocationConstants.BirokratExeFileName)))
                {
                    var ad = new List<string>() { betaExe + TextConstants.DELIMITER + TextConstants.POSTFIX_OK };

                    if (betaExe == BiroLocationConstants.BirokratExeFileName)
                        return new ResultRecord(true, GetType().Name, ad[0]);

                    foreach (var b in birokrats)
                    {
                        var file = _pathSystem.GetFileName(b);
                        if (file.Equals(betaExe, StringComparison.OrdinalIgnoreCase))
                            ad.Add(file + TextConstants.DELIMITER + "Beta" + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
                        else
                            ad.Add(file);
                    }
                    return new ResultRecord(true, GetType().Name, ad.ToArray());
                }

                return new ResultRecord(false, GetType().Name, betaExe);
            });

            var record = await task;
            return record;
        }

        private string GetAndRepairInstancePoolSettings()
        {
            var settingsFile = Path.Combine(_sourceDirectory, "biro_instance_pool", BiroNextConstants.NextSettingsSecretsFileName);
            Dictionary<string, string> settings;

            if (!File.Exists(settingsFile))
                settings = GenerateSettingsFile(settingsFile);
            else
                settings = _settingsLoader.LoadSettings(settingsFile);

            if (!settings.TryGetValue(BETA_KEY, out var betaExe))
            {
                var dict = GenerateSettingsFile(settingsFile);
                betaExe = dict[BETA_KEY];
            }

            return betaExe;
        }

        private Dictionary<string, string> GenerateSettingsFile(string path)
        {
            var dict = new Dictionary<string, string>()
            {
                { BETA_KEY, "Birokrat.exe" }
            };
            _settingsLoader.SaveSettings(path, dict);
            return dict;
        }
    }
}

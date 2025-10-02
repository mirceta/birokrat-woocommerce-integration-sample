using BirokratNext;
using common_ops.diagnostics.Checks.Api.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Api.Checks
{
    /// <summary>
    /// Will check Artikle calls CRUD operations. Can artikel be created, updated and deleted. Will retrurn true if all operations succeed.
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: result for each operation in format: NameOfOperation||Postfix.
    /// Separated with <c>||</c>. Example: Create Call||OK\r\Update Call||OK\r\Delete Call||OK</para>
    /// 
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR </para>
    /// </summary>
    public class Api_Artikel_Check : ICheck
    {
        private readonly IApiHelper _apiHelper;
        private string _apiKey;

        /// <summary>
        /// <inheritdoc cref="Api_Artikel_Check"/>
        /// </summary>
        public Api_Artikel_Check(IApiHelper apiHelper, string apiKey = "")
        {
            _apiHelper = apiHelper;
            _apiKey = apiKey;
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
            var apiClient = _apiHelper.BuildClient(_apiKey);
            var results = await CRUD_ArtikelTest(apiClient);

            if (!results.Any())
                return new ResultRecord(false, GetType().Name, TextConstants.POSTFIX_ERROR);

            if (results.All(x => x.EndsWith(TextConstants.POSTFIX_OK)))
                return new ResultRecord(true, GetType().Name, results.ToArray());
            return new ResultRecord(false, GetType().Name, results.ToArray());
        }

        private async Task<List<string>> CRUD_ArtikelTest(ApiClientV2 apiClient)
        {
            var description = "Update was ok";
            var sifraArtikla = "1337TS";
            var resultsLog = new List<string>();

            var artikel = new Dictionary<string, object>()
            {
                { "txtOpis", "Next TEST" },
                { "txtAlternativniOpis", "..." },
                { "txtEnota", "kos" },
                { "txtSifraArtikla", sifraArtikla },
                { "SifraDavka", "1    22 DDV osnovna stopnja                     " },
                { "PCBrezPD", "11,0000" },
            };

            try
            {
                async Task<Dictionary<string, object>> GetParams(string id)
                {
                    var res = await apiClient.sifrant.UpdateParameters("sifranti/artikli/prodajniartikli-storitve", id);

                    return res
                        .GroupBy(x => x.Koda)
                        .Select(g => g.First())
                        .ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);
                }

                var result2 = await apiClient.sifrant.Create("sifranti/artikli/prodajniartikli-storitve", artikel);

                var bres = result2.ToString() == sifraArtikla || DoesArtikelAlreadyExist(result2.ToString(), sifraArtikla);
                resultsLog.Add(_apiHelper.BuildArgumentFromResult("Create Call", bres));

                var result3 = await apiClient.sifrant.UpdateParameters("sifranti/artikli/prodajniartikli-storitve", sifraArtikla);
                var arguments = await GetParams(sifraArtikla);
                arguments["txtAlternativniOpis"] = description;

                var res_upd = await apiClient.sifrant.Update("sifranti/artikli/prodajniartikli-storitve", arguments);

                var check = await GetParams(sifraArtikla);
                if (check.TryGetValue("txtAlternativniOpis", out var value))
                {
                    resultsLog.Add(_apiHelper.BuildArgumentFromResult("Update Call", value.ToString() == description));
                }

                var res_del = await apiClient.sifrant.Delete("sifranti/artikli/prodajniartikli-storitve", sifraArtikla);
                resultsLog.Add(_apiHelper.BuildArgumentFromResult("Delete Call", res_del.Trim() == "1"));
            }
            catch (Exception ex)
            {
                resultsLog.Add(_apiHelper.BuildExceptionErrorArgument(ex));
                resultsLog.Add("If this tests fail but Next is running try log into birokrat.exe with default taxNumber. This will force upgrade on databases");
                resultsLog.Add("Additionally log in Birokrat as 0000000 and update licence: Birokrat -> Servis -> Prenesi licenco iz interneta. ");
            }
            return resultsLog;
        }

        private bool DoesArtikelAlreadyExist(string source, string sifraArtikla)
        {
            if (!source.Contains("Artikel z oznako " + sifraArtikla))
                return false;
            if (!source.Contains("že obstaja!"))
                return false;
            return true;
        }
    }
}

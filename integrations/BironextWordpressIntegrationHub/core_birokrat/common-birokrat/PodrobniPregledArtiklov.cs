using BirokratNext;
using BirokratNext.api_clientv2;
using BirokratNext.Exceptions;
using BironextWordpressIntegrationHub;
using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWooHub.logic.integration;
using core.tools.zalogaretriever;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.common_birokrat {

    interface IParameterModifyOp
    {
        void ModifyParameters(List<PLParameterResponseRecord> pars, Dictionary<string, object> postback);
        void ModifyDataAfterFetch(List<Dictionary<string, object>> data);
    }
    public class PodrobniPregledPartnerjev
    {

        List<IParameterModifyOp> ops;
        public PodrobniPregledPartnerjev()
        {
            ops = new List<IParameterModifyOp>();
        }

        public async Task<List<Dictionary<string, object>>> GetPodrobniPregledArtiklov(IApiClientV2 client)
        {

            while (true)
            {
                try
                {
                    var pars = await client.cumulative.Parametri("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev");
                    Dictionary<string, object> postback = pars.ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);

                    ops.ForEach(op => op.ModifyParameters(pars, postback));
                    var data = await client.cumulative.Podatki("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev", postback);
                    ops.ForEach(op => op.ModifyDataAfterFetch(data));

                    return data;
                }
                catch (BironextRestartException ex)
                {

                }
            }
        }
    }
    public class PodrobniPregledArtiklov {

        List<IParameterModifyOp> ops;
        public PodrobniPregledArtiklov() {
            ops = new List<IParameterModifyOp>();
            ops.Add(new BasicParameterModifyOp());
        }
        
        public void AddAdditionalParameters(Dictionary<string, object> additionalAttributes = null) {
            ops.Add(new AdditionalParametersModifyOp(additionalAttributes));
        }

        public void ModifyParameters(Dictionary<string, object> parameters) {
            ops.Add(new ConcreteParameterModifyOp(parameters));
        }

        public async Task<List<Dictionary<string, object>>> GetPodrobniPregledArtiklov(IApiClientV2 client) {

            while (true) {
                try {
                    var pars = await client.cumulative.Parametri("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev");
                    Dictionary<string, object> postback = pars.ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);
                    var tmp = JsonConvert.SerializeObject(postback);

                    ops.ForEach(op => op.ModifyParameters(pars, postback));
                    var data = await client.cumulative.Podatki("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev", postback);
                    ops.ForEach(op => op.ModifyDataAfterFetch(data));

                    return data;
                } catch (BironextRestartException ex) {

                }
            }
        }

        class BasicParameterModifyOp : IParameterModifyOp {
            public void ModifyDataAfterFetch(List<Dictionary<string, object>> data) { }
            public void ModifyParameters(List<PLParameterResponseRecord> pars, Dictionary<string, object> postback) {
                postback["SpremembaOdDatuma"] = "";
                postback["SpremembaOdUre"] = "";
                postback["SpremembaDoUre"] = "";
            }
        }

        class ConcreteParameterModifyOp : IParameterModifyOp {

            Dictionary<string, object> modifiedParams;
            public ConcreteParameterModifyOp(Dictionary<string, object> modifiedParams) {
                this.modifiedParams = modifiedParams;
            }

            public void ModifyDataAfterFetch(List<Dictionary<string, object>> data) {}

            public void ModifyParameters(List<PLParameterResponseRecord> pars, Dictionary<string, object> postback) {
                foreach (var x in modifiedParams) {
                    postback[x.Key] = x.Value;
                }
            }
        }

        class AdditionalParametersModifyOp : IParameterModifyOp {

            Dictionary<string, object> additionalAttributes;
            Dictionary<string, string> columnNameChange;

            public AdditionalParametersModifyOp(Dictionary<string, object> additionalAttributes) {
                this.additionalAttributes = additionalAttributes;
            }

            public void ModifyDataAfterFetch(List<Dictionary<string, object>> data) {
                // Make sure that additional attributes have column name the same as koda
                // and not as opis!!!
                foreach (var row in data) {
                    foreach (var kvp in columnNameChange) {
                        object tmp = row[kvp.Key];
                        row.Remove(kvp.Key);
                        row[kvp.Value] = tmp;
                    }
                }
            }

            public void ModifyParameters(List<PLParameterResponseRecord> pars, Dictionary<string, object> postback) {
                columnNameChange = AddAdditionalAttrsAsParams(additionalAttributes, pars, postback);
            }

            private Dictionary<string, string> AddAdditionalAttrsAsParams(Dictionary<string, object> additionalAttributes, List<PLParameterResponseRecord> pars, Dictionary<string, object> postback) {

                string prefix_of_addattrs = "P§";

                var columnNameChange = new Dictionary<string, string>();
                if (additionalAttributes != null) {
                    additionalAttributes.ToList().ForEach(x => {

                        string currKey = prefix_of_addattrs + x.Key; // this is how it is in podrobni pregled params!

                        if (!postback.Keys.Contains(currKey))
                            throw new Exception($"A key ({x.Key}) was contained in queryTerms that did not appear in PL (parameters)");
                        postback[currKey] = x.Value;

                        var param = pars.Where(y => y.Koda == x.Key).Single();
                        columnNameChange[param.Opis] = param.Koda;
                    });
                }

                return columnNameChange;
            }
        }

    }
}

using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.order_operations.pl {


    class DocumentParameterManager {

        IApiClientV2 client;
        string apiPath;
        string documentNumber;
        public DocumentParameterManager(IApiClientV2 client, string documentApiPath, string documentNumber) {
            this.client = client;
            this.apiPath = documentApiPath;
            this.documentNumber = documentNumber;
        }

        Dictionary<string, object> currentParams;
        Dictionary<string, object> stagedParams;
        Dictionary<string, object> expectedParams;

        public async Task ExecuteChain(WoocommerceOrder order, Dictionary<string, object> data, List<DocumentParameterCommand> operation) {

            await InitializeState();
            foreach (var op in operation) {
                 await Execute(op, order, data);
            }
            await VerifyCorrect();
        }

        private async Task InitializeState() {
            var pake = (await client.document.UpdateParameters(apiPath, documentNumber));
            currentParams = pake
                .GroupBy(x => x.Koda)
               .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            stagedParams = new Dictionary<string, object>();
            expectedParams = new Dictionary<string, object>();
        }

        private async Task Execute(DocumentParameterCommand command, WoocommerceOrder order, Dictionary<string, object> data) {
            if (command.Condition.Is(order, data)) {

                if (command.RequiresRefresh) {
                    await Refresh();
                }

                string value = "";
                switch (command.Operation) {
                    case ParameterOperation.APPEND:
                        value = (string)currentParams[command.FieldName];
                        value += command.Value.Get(order, data);
                        break;
                    case ParameterOperation.REPLACE:
                        value = (string)currentParams[command.FieldName];
                        value = value.Replace(command.Value.Get(order, data), command.ReplaceWith);
                        break;
                    case ParameterOperation.SET:
                        value = command.Value.Get(order, data);
                        break;
                }
                currentParams[command.FieldName] = value;
                stagedParams[command.FieldName] = value;
                expectedParams[command.FieldName] = value;
            }
        }
        private async Task VerifyCorrect() {
            currentParams = (await client.document.Update(apiPath, documentNumber, stagedParams))
                                                .GroupBy(x => x.Koda)
                                                .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            ThrowIfParametersNotConsistent(stagedParams);
        }

        #region [auxiliary]
        private async Task Refresh() {
            if (stagedParams.Keys.Count > 0) {
                currentParams = (await client.document.Update(apiPath, documentNumber, stagedParams))
                                    .GroupBy(x => x.Koda)
                                    .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
                ThrowIfParametersNotConsistent(stagedParams);
                stagedParams = new Dictionary<string, object>();
            }
        }

        private void ThrowIfParametersNotConsistent(Dictionary<string, object> stagedParamsOnLastUpdate) {
            foreach (var key in stagedParams.Keys) {
                string expected = (string)expectedParams[key];
                string current = (string)currentParams[key];
                if (expected != current) {
                    throw new Exception($"After calling {apiPath}/{documentNumber}, updating {DicToString(stagedParamsOnLastUpdate)} expected {key} to be {expectedParams[key]}, but Birokrat says that it is {currentParams[key]}");
                }
            }
        }

        private static string DicToString(Dictionary<string, object> dic) {
            string some = "";
            foreach (var key in dic.Keys) {
                some += $"{key}: {(string)dic[key]};";
            }
            return some;
        }
        #endregion
    }


    public enum ParameterOperation { 
        APPEND = 1,
        SET = 2,
        REPLACE = 3
    }
}

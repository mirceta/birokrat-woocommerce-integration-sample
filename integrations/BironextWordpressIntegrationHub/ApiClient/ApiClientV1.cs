using Birokrat.Next.ApiClient.Utils;
using BirokratNext.Exceptions;
using BirokratNext.Models;
using BirokratNext.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BirokratNext
{
    public class ApiClientV1 : IApiClient, IDisposable
    {
        private const string DEFAULT_API_ADDRESS = "https://next.birokrat.si/api/";
        private Token token;
        private string taxNumber;
        private string companyPassword;
        private string userName;
        private string password;

        public ApiClientV1(string apiAddress, string taxNumber, string companyPassword, string username, string password)
        {
            HttpClient = new HttpClient { BaseAddress = new Uri(apiAddress) };
            DeviceIdentifier = Networking.GetMacAddress().ToString();
            this.taxNumber = taxNumber;
            this.companyPassword = companyPassword;
            this.userName = username;
            this.password = password;
         }

        public virtual HttpClient HttpClient { get; private set; }

        public virtual Token Token {
            get { return token; }
            private set {
                token = value;
                if (value != null)
                {
                    HttpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(value.TokenType, value.AccessToken);
                }
            }
        }

        public virtual string DeviceIdentifier { get; private set; }

        public async Task Start() {
            await Authenticate(
                taxNumber: taxNumber,
                companyPassword: companyPassword,
                userName: userName,
                password: password
            );
        }

        public async Task<object> Test()
        {
            var response = await HttpClient.GetAsync($"dll/test");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<Parameter>> KumulativaParametri(string key, List<Parameter> parameters = null) =>
            CreateParameterListFromResponseParameters(await Call("Cumulative", "Parameters", key, parameters));

        public async Task<List<Parameter>> SifrantParametri(string key, List<Parameter> parameters = null) =>
            CreateParameterListFromResponseParameters(await Call("CodeList", "Parameters", key, parameters));

        public async Task<DataSet> KumulativaPodatki(string key, List<Parameter> parameters = null) =>
            CreateDataSetFromExcelResponseData(await Call("Cumulative", "Data", key, parameters), key);

        public async Task<string> KumulativaPodatkiJson(string key, List<Parameter> parameters = null) =>
            Serializer.ToJson(await KumulativaPodatki(key, parameters), indented: true);

        public async Task<DataSet> SifrantPodatki(string key, List<Parameter> parameters = null) =>
            CreateDataSetFromJsonResponseData(await Call("CodeList", "Data", key, parameters), key);

        public async Task<string> SifrantPodatkiJson(string key, List<Parameter> parameters = null) =>
            Serializer.ToJson(await SifrantPodatki(key, parameters), indented: true);

        private async Task<object> Call(string type, string subtype, string key, List<Parameter> parameters)
        {
            var content = Serializer.ToJson(new
            {
                Method = new { Type = type, Subtype = subtype, Key = key },
                Parameters = parameters ?? new List<Parameter> { }
            });

            try
            {
                var response = await HttpClient.PostAsync($"dll/{type}",
                    new StringContent(content, Encoding.UTF8, "application/json"));
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new ApiException($"Birokrat Next API call failed: {key}.");
            }
        }

        private List<Parameter> CreateParameterListFromResponseParameters(object data)
        {
            var parametersObject = Serializer.FromJsonAnonymous((string)data)["parameters"];
            var parameterList = new List<Parameter>();

            foreach (var group in parametersObject)
            {
                foreach (var element in group["elements"])
                {
                    var parameter = Serializer.FromJson<Parameter>(element);
                    parameterList.Add(parameter);
                }
            }

            return parameterList;
        }

        private DataSet CreateDataSetFromExcelResponseData(object data, string key)
        {
            var encodedExcelData = Serializer.FromJsonAnonymous((string)data).Value<string>("excelData");
            var excelData = Convert.FromBase64String(encodedExcelData);
            return DataSetUtils.CreateFromExcel(excelData, key);
        }

        private DataSet CreateDataSetFromJsonResponseData(object data, string key)
        {
            var dataObject = Serializer.FromJsonAnonymous((string)data)["data"];
            return DataSetUtils.CreateFromJson(dataObject, key);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            HttpClient = null;
            Token = null;
        }

        private async Task Authenticate(string taxNumber, string companyPassword, string userName, string password) {
            var content = Serializer.ToJson(new {
                TaxNumber = taxNumber,
                CompanyPassword = companyPassword,
                UserName = userName,
                Password = password,
                DeviceIdentifier
            });

            try {
                var response = await HttpClient.PostAsync("login",
                    new StringContent(content, Encoding.UTF8, "application/json"));
                var tokenData = await response.Content.ReadAsStringAsync();
                Token = Serializer.FromJson<Token>(tokenData);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw new AuthenticationException("Authentication with Birokrat Next API failed.");
            }
        }
    }
}

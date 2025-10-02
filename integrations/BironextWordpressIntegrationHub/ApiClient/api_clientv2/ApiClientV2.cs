using Birokrat.Next.ApiClient.Utils;
using BirokratNext.api_clientv2;
using BirokratNext.Exceptions;
using BirokratNext.Models;
using BirokratNext.Utils;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BirokratNext {

    public class UtilitiesCalls : FunctionalityCall, IUtilitiesCalls
    {
        public UtilitiesCalls(IHttpClientFactory factory, HttpClient client, IMyLogger logger) : base(client, factory, logger)
        {
        }

        public async Task<string> DavcnaStevilka(string path, string davcna)
        {
            path = Path.Combine("v2", path, davcna);
            string some = (string)await HttpGet(path);
            return some;
        }
    }

    public class HttpClientFactory : IHttpClientFactory
    {

        string address;
        string apikey;
        public HttpClientFactory(string address, string apikey)
        {
            this.address = address;
            this.apikey = apikey;
        }

        public HttpClient Create()
        {
            var client = new HttpClient { BaseAddress = new Uri(address) };
            client.DefaultRequestHeaders.Add("X-API-KEY", apikey);
            return client;
        }
    }

    public class ApiClientV2 : IDisposable, IApiClientV2
    {

        public IDocumentCalls document { get; }
        public ICumulativeCalls cumulative { get; }
        public ISifrantCalls sifrant { get; }
        public IUtilitiesCalls utilities { get; }
        public IHttpClientFactory httpClientFactory { get; }
        public virtual HttpClient HttpClient { get; private set; }
        private string apiKey;
        private string apiAddress;
        public virtual string ApiKey
        {
            get
            {
                return apiKey;
            }
            set
            {
                apiKey = value;
                HttpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            }
        }

        private int timeoutSeconds;

        IMyLogger logger;
        public ApiClientV2(string apiAddress, string apiKey, int timeoutSeconds = 3600)
        {
            this.apiAddress = apiAddress;
            this.apiKey = apiKey;
            httpClientFactory = new HttpClientFactory(apiAddress, apiKey);
            HttpClient = httpClientFactory.Create();
            this.timeoutSeconds = timeoutSeconds;
            this.logger = new ConsoleMyLogger();
            HttpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            document = new DocumentCalls(httpClientFactory, HttpClient, logger);
            cumulative = new CumulativeCalls(httpClientFactory, HttpClient, logger);
            sifrant = new SifrantCalls(httpClientFactory, HttpClient, logger);
            utilities = new UtilitiesCalls(httpClientFactory, HttpClient, logger);


        }

        public void SetLogger(IMyLogger logger)
        {
            this.logger = logger;
            document.SetLogger(logger);
            cumulative.SetLogger(logger);
            sifrant.SetLogger(logger);
            utilities.SetLogger(logger);
        }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public async Task<object> Logout()
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync("v2/restart");
                return "ok";
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw new ApiException($"Birokrat Next API call failed.");
            }
        }

        public async Task<object> Test()
        {
            var tmp = new Dictionary<string, object>();
            tmp["someKey"] = "someValue";
            var content = Serializer.ToJson(tmp);

            var response = await HttpClient.PostAsync(@"v2\poslovanje\racuni\kumulativnipregled\parameters",
                new StringContent(content, Encoding.UTF8, "application/json"));
            return await response.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            HttpClient = null;
        }
    }
}

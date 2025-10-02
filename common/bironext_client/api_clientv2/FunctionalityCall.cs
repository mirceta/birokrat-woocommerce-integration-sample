using BirokratNext.Exceptions;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2
{
    public class FunctionalityCall
    {
        public virtual HttpClient HttpClient { get; private set; }
        public IHttpClientFactory HttpClientFactory { get; }

        IMyLogger logger;
        public FunctionalityCall(HttpClient client, IHttpClientFactory factory, IMyLogger logger) {
            HttpClient = client;
            HttpClientFactory = factory;
            this.logger = logger;
        }

        public void SetLogger(IMyLogger logger) {
            this.logger = logger;
        }
        
        public async Task<object> HttpDelete(string path) {
            try {
                HttpResponseMessage response = null;
                response = await HttpClient.DeleteAsync($"{path}");
                string cont = await response.Content.ReadAsStringAsync();
                cont = response.StatusCode == HttpStatusCode.OK ? "" : cont;
                logger.LogInformation(response.StatusCode + $" {path} {cont}");
                return await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                logger.LogError(ex.ToString());
                throw new ApiException($"Birokrat Next API call failed: {path}.");
            }
        }

        public async Task<object> HttpPost(string path, string content = "") {

            string cont = "";
            try {
                HttpResponseMessage response = null;
                do {
                    var some = new StringContent(content, Encoding.UTF8, "application/json");
                    response = await HttpClient.PostAsync($"{path}", some);
                    cont = await response.Content.ReadAsStringAsync();
                    cont = response.StatusCode == HttpStatusCode.OK ? "" : cont;
                    logger.LogInformation(response.StatusCode + $" {path} {cont}");
                    if (response.StatusCode == HttpStatusCode.GatewayTimeout) {
                        var res = await HttpClient.GetAsync(Path.Combine("v2", "restart"));
                        logger.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        logger.LogError("RESTART: " + await res.Content.ReadAsStringAsync());
                        logger.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        throw new BironextRestartException("");
                    }
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest) break;
                    await Task.Delay(3000);
                } while (response.StatusCode == HttpStatusCode.Accepted);
                if (response.StatusCode == HttpStatusCode.InternalServerError) {
                    throw new ApiException(cont);
                }
                string result = await response.Content.ReadAsStringAsync();
                if (result == "Concurrent requests are not allowed") {
                    throw new ConcurrentRequestsNotAllowedException();
                }
                return result;
            } catch (Exception ex) {
                if (ex.GetType() == typeof(BironextRestartException))
                    throw (BironextRestartException)ex;
                else if (ex.GetType() == typeof(ConcurrentRequestsNotAllowedException))
                    throw (ConcurrentRequestsNotAllowedException)ex;
                else {
                    logger.LogError(ex.ToString());
                    throw new ApiException($"Birokrat Next API call failed: {path}. {ex.ToString()}");
                }
            }
        }

        public async Task<object> HttpGet(string path, HttpClient client = null) {
            var useclient = HttpClient;
            if (client != null)
                useclient = client;
            try {
                HttpResponseMessage response = null;
                do {
                    response = await useclient.GetAsync($"{path}");
                    string cont = await response.Content.ReadAsStringAsync();
                    cont = response.StatusCode == HttpStatusCode.OK ? "" : cont;
                    logger.LogError(response.StatusCode + $" {path} {cont}");
                    if (response.StatusCode == HttpStatusCode.OK) break;
                    await Task.Delay(3000);
                } while (response.StatusCode == HttpStatusCode.Accepted);
                if (response.StatusCode == HttpStatusCode.InternalServerError) {

                    string content = "";
                    try {
                        content = await response.Content.ReadAsStringAsync();
                    } catch (Exception ex) { }
                    logger.LogWarning($"Internal server error! {content}");
                    throw new ApiException($"Internal server error! {content}");
                }
                return await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                logger.LogError(ex.ToString());
                throw new ApiException($"Birokrat Next API call failed: {path}.");
            }
        }
    }
}

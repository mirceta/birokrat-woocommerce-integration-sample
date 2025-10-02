using BirokratNext.Exceptions;
using BirokratNext.Models;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace BirokratNext.Tests
{
    [ExcludeFromCodeCoverage]
    public class TestParameters : IDisposable
    {
        private const string API_ADDRESS = "http://localhost/api/";
        public const string TOKEN_RESPONSE_JSON =
            "{\"access_token\":\"foo\",\"expires_in\":3600,\"token_type\":\"Bearer\",\"refresh_token\":\"bar\"}";

        Mock<ApiClientV1> mockApiClient;
        MockHttpMessageHandler mockHttp;

        public TestParameters()
        {
            mockApiClient = new Mock<ApiClientV1>
            {
                CallBase = true
            };
            mockHttp = new MockHttpMessageHandler();
            var httpClient = mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri(API_ADDRESS);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "foo");
            mockApiClient.Setup(x => x.HttpClient).Returns(httpClient);
        }

        public void Dispose()
        {
            mockApiClient.Object.Dispose();
            mockApiClient = null;
            mockHttp.Dispose();
            mockHttp = null;
        }

        [Fact]
        public async Task TestKumulativaSuccess()
        {
            var request = mockHttp
                .When($"{API_ADDRESS}dll/Cumulative")
                .WithContent("{\"Method\":{\"Type\":\"Cumulative\",\"Subtype\":\"Parameters\",\"Key\":\"goo\"},\"Parameters\":[]}")
                .Respond("application/json", ParametersFixtures.JSON_RESPONSE);

            var requestWithParameters = mockHttp
                .When($"{API_ADDRESS}dll/Cumulative")
                .WithContent("{\"Method\":{\"Type\":\"Cumulative\",\"Subtype\":\"Parameters\",\"Key\":\"hoo\"},\"Parameters\":[{\"Type\":\"Input\",\"Code\":\"xyz\",\"Value\":\"bla\"}]}")
                .Respond("application/json", ParametersFixtures.JSON_RESPONSE);

            var parameters = await mockApiClient.Object.KumulativaParametri("goo");
            await mockApiClient.Object.KumulativaParametri("hoo", new List<Parameter>
            {
                new Parameter { Type = "Input", Code = "xyz", Value = "bla" }
            });

            Assert.Equal(1, mockHttp.GetMatchCount(request));
            Assert.Equal(1, mockHttp.GetMatchCount(requestWithParameters));

            Assert.Equal(25, parameters.Count);

            var parameter = parameters[10];
            Assert.Equal("Vrsta partnerjev", parameter.Label);
            Assert.Equal("Select", parameter.Type);
            Assert.Equal("VrstaPartnerjev", parameter.Code);
            Assert.Equal("", parameter.Value);
            Assert.Equal(new List<string> { "Kupci", "" }, parameter.DataSet);

            Assert.Equal("Bearer", mockApiClient.Object.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("foo", mockApiClient.Object.HttpClient.DefaultRequestHeaders.Authorization.Parameter);

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestKumulativaFailure()
        {
            // we intentionally remove the base address from the HttpClient to trigger InvalidOperationException
            var httpClient = mockHttp.ToHttpClient();
            mockApiClient.Setup(x => x.HttpClient).Returns(httpClient);
            var request = mockHttp.When($"dll/Cumulative");

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => mockApiClient.Object.KumulativaParametri("goo")
            );

            Assert.Equal(0, mockHttp.GetMatchCount(request));

            Assert.Equal("Birokrat Next API call failed: goo.", exception.Message);

            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}

using BirokratNext.Exceptions;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace BirokratNext.Tests
{
    [ExcludeFromCodeCoverage]
    public class TestAuthentication : IDisposable
    {
        private const string API_ADDRESS = "http://localhost/api/";

        Mock<ApiClientV1> mockApiClient;
        MockHttpMessageHandler mockHttp;

        public TestAuthentication()
        {
            mockApiClient = new Mock<ApiClientV1>
            {
                CallBase = true
            };
            mockHttp = new MockHttpMessageHandler();
            var httpClient = mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri(API_ADDRESS);
            mockApiClient.Setup(x => x.HttpClient).Returns(httpClient);
            mockApiClient.Setup(x => x.DeviceIdentifier).Returns("MAC");
        }

        public void Dispose()
        {
            mockApiClient.Object.Dispose();
            mockApiClient = null;
            mockHttp.Dispose();
            mockHttp = null;
        }

        [Fact]
        public async Task TestSuccess()
        {
            var request = mockHttp
                .When($"{API_ADDRESS}login")
                .WithContent("{\"TaxNumber\":\"12345678\",\"CompanyPassword\":\"pass1\",\"UserName\":\"abc\",\"Password\":\"pass2\",\"DeviceIdentifier\":\"MAC\"}")
                .Respond("application/json", AuthenticationFixtures.TOKEN_RESPONSE_JSON);


            Assert.Equal(1, mockHttp.GetMatchCount(request));

            Assert.Equal("foo", mockApiClient.Object.Token.AccessToken);
            Assert.Equal(3600, mockApiClient.Object.Token.ExpiresIn);
            Assert.Equal("Bearer", mockApiClient.Object.Token.TokenType);
            Assert.Equal("bar", mockApiClient.Object.Token.RefreshToken);

            Assert.Equal("Bearer", mockApiClient.Object.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("foo", mockApiClient.Object.HttpClient.DefaultRequestHeaders.Authorization.Parameter);

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestFailure()
        {
            var request = mockHttp
                .When($"{API_ADDRESS}login")
                .WithContent("{\"TaxNumber\":\"87654321\",\"CompanyPassword\":\"1pass\",\"UserName\":\"def\",\"Password\":\"2pass\",\"DeviceIdentifier\":\"MAC\"}")
                .Respond("application/json", "some random unparsable string");

            Assert.Equal(1, mockHttp.GetMatchCount(request));

            Assert.Null(mockApiClient.Object.Token);
            Assert.Null(mockApiClient.Object.HttpClient.DefaultRequestHeaders.Authorization);

            //Assert.Equal("Authentication with Birokrat Next API failed.", exception.Message);

            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}

namespace Scaffold.HttpClients.UnitTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class ExampleHttpClientUnitTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("test")]
        [InlineData("/test")]
        public async Task When_InvokingGetWithPath_Expect_RequestUri(string path)
        {
            // Arrange
            string content = Guid.NewGuid().ToString();

            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new StringContent(content),
                StatusCode = HttpStatusCode.OK,
            };

            HttpRequestHandler httpRequestHandler = new HttpRequestHandler(response);

            HttpResponseMessage result;

            // Act
            using (HttpClient httpClient = new HttpClient(httpRequestHandler))
            {
                ExampleHttpClient exampleHttpClient = new ExampleHttpClient(httpClient);
                result = await exampleHttpClient.Get(path);
            }

            // Assert
            Assert.Equal(HttpMethod.Get, result.RequestMessage.Method);
            Assert.EndsWith(path ?? string.Empty, result.RequestMessage.RequestUri.ToString());
            Assert.Equal(content, await result.Content.ReadAsStringAsync());
        }

        private class HttpRequestHandler : DelegatingHandler
        {
            private readonly HttpResponseMessage response;

            public HttpRequestHandler(HttpResponseMessage response)
            {
                this.response = response;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.response.RequestMessage = request;
                return await Task.FromResult(this.response);
            }
        }
    }
}

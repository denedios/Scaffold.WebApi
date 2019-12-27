namespace Scaffold.WebApi.UnitTests.HttpMessageHandlers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using OpenTracing;
    using OpenTracing.Mock;
    using Scaffold.WebApi.HttpMessageHandlers;
    using Xunit;

    public class OpenTracingSpanTaggingHttpMessageHandlerUnitTests
    {
        [Theory]
        [InlineData(499, false)]
        [InlineData(500, true)]
        [InlineData(501, true)]
        public async Task When_SendingAsync_Expect_SetTag(int statusCode, bool expectedError)
        {
            // Arrange
            ServiceCollection services = new ServiceCollection();
            services.AddScoped<ITracer, MockTracer>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITracer tracer = serviceProvider.GetRequiredService<ITracer>();

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider },
            };

            OpenTracingSpanTaggingHttpMessageHandler handler = new OpenTracingSpanTaggingHttpMessageHandler(httpContextAccessor)
            {
                InnerHandler = new InnerHandler(statusCode),
            };

            // Act
            using (tracer.BuildSpan("SomeWork").StartActive())
            using (HttpClient client = new HttpClient(handler))
            {
                await client.GetAsync(new Uri("http://localhost"));
            }

            // Assert
            MockTracer mockTracer = Assert.IsType<MockTracer>(tracer);
            MockSpan mockSpan = Assert.Single(mockTracer.FinishedSpans());

            if (mockSpan.Tags.ContainsKey("error"))
            {
                Assert.Equal(expectedError, mockSpan.Tags["error"]);
            }

            Assert.Equal(expectedError, mockSpan.Tags.ContainsKey("error"));
        }

        private class InnerHandler : DelegatingHandler
        {
            private readonly int statusCode;

            public InnerHandler(int statusCode)
            {
                this.statusCode = statusCode;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return await Task.FromResult(new HttpResponseMessage { StatusCode = (HttpStatusCode)this.statusCode });
            }
        }
    }
}

namespace Scaffold.WebApi.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using Scaffold.WebApi.Controllers;
    using Xunit;

    public class TracingDemoControllerUnitTests
    {
        private readonly ActionContext actionContext;

        public TracingDemoControllerUnitTests()
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            httpContext.Request.Scheme = "http";

            this.actionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor(),
                HttpContext = httpContext,
                RouteData = new RouteData(),
            };
        }

        public class Proxy : TracingDemoControllerUnitTests
        {
            [Fact]
            public async Task When_SayingHelloToName_Expect_HelloMessage()
            {
                // Arrange
                HttpClient httpClient = new HttpClient(new InnerHandler());
                TracingDemoController.Client tracingDemoClient = new TracingDemoController.Client(httpClient);

                TracingDemoController controller = new TracingDemoController(tracingDemoClient)
                {
                    ControllerContext = new ControllerContext(this.actionContext),
                };

                string name = Guid.NewGuid().ToString();

                // Act
                string result = await controller.Proxy(name);

                // Assert
                Assert.Equal($"Hello, {name}!", result);
            }

            [Fact]
            public async Task When_SayingHelloToNullName_Expect_HelloMessage()
            {
                // Arrange
                HttpClient httpClient = new HttpClient(new InnerHandler());
                TracingDemoController.Client tracingDemoClient = new TracingDemoController.Client(httpClient);

                TracingDemoController controller = new TracingDemoController(tracingDemoClient)
                {
                    ControllerContext = new ControllerContext(this.actionContext),
                };

                // Act
                string result = await controller.Proxy(null);

                // Assert
                Assert.Equal($"Hello, random!", result);
            }
        }

        public class Hello
        {
            [Fact]
            public void When_SayingHelloToName_Expect_HelloMessage()
            {
                // Arrange
                TracingDemoController controller = new TracingDemoController(null!);
                string name = Guid.NewGuid().ToString();

                // Act
                string result = controller.Hello(name);

                // Assert
                Assert.Equal($"Hello, {name}!", result);
            }

            [Fact]
            public void When_SayingHelloToNullName_Expect_HelloMessage()
            {
                // Arrange
                TracingDemoController controller = new TracingDemoController(null!);

                // Act
                string result = controller.Hello(null);

                // Assert
                Assert.Equal($"Hello, random!", result);
            }
        }

        private class InnerHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Dictionary<string, StringValues> queryParameters = QueryHelpers.ParseQuery(request.RequestUri.Query);

                return await Task.FromResult(new HttpResponseMessage
                {
                    Content = new StringContent($"Hello, {queryParameters["name"]}!"),
                });
            }
        }
    }
}

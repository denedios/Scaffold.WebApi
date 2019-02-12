# Health Checks #

Scaffold.WebApi uses a basic implementation of the [Health Check Middleware](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks) included in ASP.NET Core. The health status of Scaffold.WebApi can be check at `/health` and returns a HTTP 200 OK response when the Web API is healthy. This health check is intended to be used by monitoring services.

## Health Checks for Monitoring Services ##

Health checks that check instances of an application are usually intended for container orchestrators and load balancers to make decisions on whether or not to put instances of an application into service or to take them out. This is the type of health check that has been implemented in Scaffold.WebApi.

## Health Check Port ##

Health checks intended for monitoring services usually only need to be exposed internally.

When you build a container image of Scaffold.WebApi and run it, the health check endpoint is exposed on port `8081` while the rest of the application remains on port `80`. Port `80` is intended to be the public port while port `8081` is the private port intended for monitoring services. You can change the ports used in the Scaffold.WebApi container image by modifying the [Dockerfile](../Sources/Scaffold.WebApi/Dockerfile) used to build it and the [docker-compose.yml](../docker-compose.yml) used to run it.

When Scaffold.WebApi is run locally, the health check endpoint is exposed on the same port as the rest of the application. For example on `http://localhost:5000/health`. Change this by modifying [launchSettings.json](../Sources/Scaffold.WebApi/Properties/launchSettings.json).
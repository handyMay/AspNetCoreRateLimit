namespace AspNetCoreRateLimit.Tests
{
    using AspNetCoreRateLimit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute.Extensions;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Extensions.Logging;
    using System.Net;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Grpc.Net.Client;

    public static class RateLimiterExtension
    {
        public static void AddRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(configuration.GetSection("ClientRateLimitPolicies"));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, MyIRateLimitConfiguration>();
        }

        public static void UseRateLimiter(this IApplicationBuilder app)
        {
            app.UseMiddleware<ForgeClientRateLimitMiddleware>();
        }
    }

    [TestClass]
    public class ThrottlingMiddlewareTests
    {
        private readonly string jsonConfig = "{ \"ClientRateLimiting\": { \"EnableEndpointRateLimiting\": true, \"HttpStatusCode\": 503, \"GeneralRules\": [ { \"Endpoint\": \"*\", \"Period\": \"5s\", \"Limit\": 1 }, { \"Endpoint\": \"*\", \"Period\": \"1m\", \"Limit\": 2 }, { \"Endpoint\": \"post://api//clients\", \"Period\": \"5m\", \"Limit\": 3 } ] }, \"ClientRateLimitPolicies\": { \"ClientRules\": [ { \"ClientId\": \"cl-key-1\", \"Rules\": [ { \"Endpoint\": \"*\", \"Period\": \"10s\", \"Limit\": 1 } ] } ] }}";

        [TestMethod]
        public async Task TestMethod1()
        {
            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration((builder, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "ClientRateLimiting:EnableEndpointRateLimiting", "True" },
                            { "ClientRateLimiting:EnableRegexRuleMatching", "True" },
                            { "ClientRateLimiting:HttpStatusCode", "503" },
                            { "ClientRateLimiting:RateLimitCounterPrefix", "Anants-RateLimitCounterPrefix" },
                            { "ClientRateLimiting:GeneralRules:0:Endpoint", "(.+):(/api/v1/).+(/experimentalevents/).+" },
                            { "ClientRateLimiting:GeneralRules:0:Period", "115m" },
                            { "ClientRateLimiting:GeneralRules:0:Limit", "1" },
                        });
                })
                .ConfigureServices((builder, serviceCollection) =>
                {
                    serviceCollection.AddRateLimiter(builder.Configuration);
                }).
                ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole(loggerConfig =>
                    {
                        loggerConfig.LogToStandardErrorThreshold = LogLevel.Trace;
                    });
                })
                .Configure(app =>
                {
                    app.UseRateLimiter();
                    //app.ApplicationServices.GetRequiredService<IClientPolicyStore>().SeedAsync().Wait();
                    app.Run(async context => await context.Response.WriteAsync("Done"));
                });

            using (var server = new TestServer(builder))
            {
                var clientPolicyStore = server.Services.GetService<IClientPolicyStore>();
                server.Services.GetService(typeof(IRateLimitCounterStore)).Should().BeOfType(typeof(MemoryCacheRateLimitCounterStore));
                clientPolicyStore.Should().NotBeNull();
                // Act
                var client = server.CreateClient();
                client.DefaultRequestHeaders.Add("X-ClientId", "anant");
                using var response = await client.GetAsync("http://localhost/api/v1/a/experimentalevents/aa");

                // Assert
                response.EnsureSuccessStatusCode();
                (await response.Content.ReadAsStringAsync()).Should().Be("Done", "get a successful response when Telemetry is applied");
                
                // Act
                using var response1 = await client.GetAsync("http://localhost/api/v1/a/experimentalevents/aaa");
                // Assert
                response1.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
                (await response1.Content.ReadAsStringAsync()).Should().NotBeNull();
            }
        }

        [TestMethod]
        public async Task TestMethodGRPC()
        {
            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration((builder, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "ClientRateLimiting:EnableEndpointRateLimiting", "True" },
                            { "ClientRateLimiting:EnableRegexRuleMatching", "True" },
                            { "ClientRateLimiting:HttpStatusCode", "503" },
                            { "ClientRateLimiting:RateLimitCounterPrefix", "Anants-RateLimitCounterPrefix" },
                            { "ClientRateLimiting:GeneralRules:0:Endpoint", "(.+):(/api/v1/).+(/experimentalevents/).+" },
                            { "ClientRateLimiting:GeneralRules:0:Period", "115m" },
                            { "ClientRateLimiting:GeneralRules:0:Limit", "1" },
                        });
                })
                .ConfigureServices((builder, serviceCollection) =>
                {
                    serviceCollection.AddRateLimiter(builder.Configuration);
                    serviceCollection.AddGrpc();
                }).
                ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole(loggerConfig =>
                    {
                        loggerConfig.LogToStandardErrorThreshold = LogLevel.Trace;
                    });
                })
                .Configure(app =>
                {
                    app.UseRateLimiter();
                    //app.ApplicationServices.GetRequiredService<IClientPolicyStore>().SeedAsync().Wait();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Communication with gRPC endpoints must be made through a gRPC client.
                        // To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
                        endpoints.MapGrpcService<TestingService>();
                    });
                    ////app.Run(async context => await context.Response.WriteAsync("Done"));
                });

            using var server = new TestServer(builder);

            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost");
            Console.WriteLine(server.BaseAddress);
            var client = new Testing.TestingClient(channel);

            var payload = new PayloadRequest();
            var resp = client.FirstAPI(payload);
            Console.WriteLine(resp.GetType().Name);
        }
    }
}

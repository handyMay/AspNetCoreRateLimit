using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreRateLimit;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit.Tests
{
    public class ForgeClientRateLimitMiddleware : ClientRateLimitMiddleware
    {
        public ForgeClientRateLimitMiddleware(
            RequestDelegate next,
            IOptions<ClientRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IClientPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<ClientRateLimitMiddleware> logger)
        : base(
            next,
            options,
            counterStore,
            policyStore,
            config,
            logger)
        { }

        public override Task<ClientRequestIdentity> ResolveIdentityAsync(HttpContext httpContext)
        {
            string clientIp = null;
            string clientId = null;

            return Task.FromResult(new ClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId ?? "anon"
            });
        }

    }
}
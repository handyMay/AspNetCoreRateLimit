using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Tests
{
    public class ForgeClientRateLimitMiddleware : ClientRateLimitMiddleware
    {
        ForgeClientRateLimitMiddleware(
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
        {}
    }
}
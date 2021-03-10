using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit.Tests
{
    internal class MyIRateLimitConfiguration : RateLimitConfiguration
    {
        public override ICounterKeyBuilder EndpointCounterKeyBuilder { get; } = new ForgePathCounterKeyBuilder();

        public MyIRateLimitConfiguration(
            IOptions<IpRateLimitOptions> ipOptions,
            IOptions<ClientRateLimitOptions> clientOptions)
            : base(ipOptions, clientOptions)
        {
        }
    }
}
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    internal class MyIRateLimitConfiguration : RateLimitConfiguration
    {
        public override ICounterKeyBuilder EndpointCounterKeyBuilder { get; } = new EndpointCounterKeyBuilder();

        public MyIRateLimitConfiguration(
            IOptions<IpRateLimitOptions> ipOptions,
            IOptions<ClientRateLimitOptions> clientOptions)
            : base(ipOptions, clientOptions)
        {
        }
    }
}
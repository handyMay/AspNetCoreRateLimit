using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit.Tests
{
    public class ForgePathCounterKeyBuilder : ICounterKeyBuilder
    {
        public string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            Console.WriteLine("ForgePathCounterKeyBuilder: building the counter key");
            return "ForgePathCounterKeyBuilder";
        }
    }
}
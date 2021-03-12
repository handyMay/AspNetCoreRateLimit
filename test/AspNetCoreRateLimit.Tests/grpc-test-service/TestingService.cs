// <copyright file="TestingServiceImpl.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace AspNetCoreRateLimit.Tests
{
    using System;
    using System.Threading.Tasks;
    using global::Grpc.Core;

    public partial class TestingService : Testing.TestingBase
    {
        public override Task<PayloadResponse> FirstAPI(PayloadRequest request, ServerCallContext context)
        {
            PayloadResponse response = new PayloadResponse();
            return Task.FromResult(response);
        }
    }
}

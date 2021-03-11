// <copyright file="TestingServiceImpl.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Outlook.Platform.Hosting.Extensions.Tests
{
    using System;
    using System.Threading.Tasks;
    using global::Grpc.Core;

    internal sealed class TestingServiceImpl : TestingService.TestingServiceBase
    {
        public override Task<Payload> SimpleMethod(Payload request, ServerCallContext context)
        {
            return Task.FromResult(request);
        }
    }
}

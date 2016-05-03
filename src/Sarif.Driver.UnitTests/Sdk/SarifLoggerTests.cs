﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class SarifLoggerTests
    {
        [Fact]
        public void CanWriteToStream()
        {
            var textWriter = new StringWriter();

            var logger = new SarifLogger(
                textWriter,
                analysisTargets: Enumerable.Empty<string>(),
                verbose: false,
                computeTargetsHash: false,
                logEnvironment: false,
                prereleaseInfo: null,
                invocationTokensToRedact: null);

            string result = textWriter.ToString();

            result.Should().NotBeNull();
        }
    }
}
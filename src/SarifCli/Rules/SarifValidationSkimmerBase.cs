﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.Json.Pointer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public abstract class SarifValidationSkimmerBase : SkimmerBase<SarifValidationContext>
    {
        protected override ResourceManager ResourceManager => RuleResources.ResourceManager;

        public override void Analyze(SarifValidationContext context)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string inputLogContents = File.ReadAllText(context.TargetUri.LocalPath);
            context.InputLogToken = JToken.Parse(inputLogContents);
            context.InputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents, settings);

            AnalyzeCore(context);
        }

        protected abstract void AnalyzeCore(SarifValidationContext context);

        protected Region GetRegionFromJPointer(string jPointerValue, SarifValidationContext context)
        {
            JsonPointer jPointer = new JsonPointer(jPointerValue);
            JToken jToken = jPointer.Evaluate(context.InputLogToken);
            IJsonLineInfo lineInfo = jToken;

            Region region = null;
            if (lineInfo.HasLineInfo())
            {
                region = new Region
                {
                    StartLine = lineInfo.LineNumber,
                    StartColumn = lineInfo.LinePosition
                };
            }

            return region;
        }
    }
}

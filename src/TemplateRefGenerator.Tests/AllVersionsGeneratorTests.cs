// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Bicep.Types.Az;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemplateRefGenerator;
using TestHelpers;

namespace TemplateRefGenerator.Tests;
[TestClass]
public class AllVersionsGeneratorTests
{
    [NotNull]
    public TestContext? TestContext { get; set; }

    [TestMethod]
    [TestCategory(BaselineHelper.BaselineTestCategory)]
    [EmbeddedFilesTestData(@"^Files/allversions/.*/.*\.md$")]
    public void AllVersions_generation_works(EmbeddedFile embeddedFile)
    {
        var baselineFolder = BaselineFolder.BuildOutputFolder(TestContext, embeddedFile);
        var markdownFile = baselineFolder.GetBaselineFile(embeddedFile.FileName);

        var streamPathParts = embeddedFile.StreamPath.Split('/').Skip(2).ToArray();
        var providerNamespace = streamPathParts[0];
        string.Join('/', streamPathParts[1..]).Should().Be("allversions.md");

        var typeProvider = new ResourceTypeProvider(new AzTypeLoader());
        var mdGenerator = new MarkdownGenerator(typeProvider);

        var groupedTypes = mdGenerator.GetGroupedTypes()
            .First(x => StringComparer.OrdinalIgnoreCase.Equals(x.ProviderNamespace, providerNamespace));

        var metadata = new MarkdownGenerator.PageMetadata(Date: DateTime.Parse("09/13/2024"), Author: "tfitzmac", MsAuthor: "tomfitz");
        var markdown = AllVersionsGenerator.GenerateMarkdown(metadata, groupedTypes);

        markdownFile.WriteToOutputFolder(markdown);
        markdownFile.ShouldHaveExpectedValue();
    }
}
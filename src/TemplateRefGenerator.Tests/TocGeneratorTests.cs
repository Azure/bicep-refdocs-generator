// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Bicep.Types.Az;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemplateRefGenerator;
using TestHelpers;

namespace TemplateRefGenerator.Tests;
[TestClass]
public class TocGeneratorTests
{
    [NotNull]
    public TestContext? TestContext { get; set; }

    [TestMethod]
    [TestCategory(BaselineHelper.BaselineTestCategory)]
    [EmbeddedFilesTestData(@"^Files/toc/.*/.*\.yml$")]
    public void AllVersions_generation_works(EmbeddedFile embeddedFile)
    {
        var baselineFolder = BaselineFolder.BuildOutputFolder(TestContext, embeddedFile);
        var tocFile = baselineFolder.GetBaselineFile(embeddedFile.FileName);

        var streamPathParts = embeddedFile.StreamPath.Split('/').Skip(2).ToArray();
        var providerNamespace = streamPathParts[0];
        string.Join('/', streamPathParts[1..]).Should().Be("toc.yml");

        var typeProvider = new ResourceTypeProvider(new AzTypeLoader());
        var mdGenerator = new MarkdownGenerator(typeProvider);

        var groupedTypes = mdGenerator.GetGroupedTypes()
            .First(x => StringComparer.OrdinalIgnoreCase.Equals(x.ProviderNamespace, providerNamespace));

        var tocContents = TocGenerator.GenerateReferenceToc(groupedTypes);

        tocFile.WriteToOutputFolder(tocContents);
        tocFile.ShouldHaveExpectedValue();
    }

    [TestMethod]
    [TestCategory(BaselineHelper.BaselineTestCategory)]
    [EmbeddedFilesTestData(@"^Files/roottoc/toc.yml$")]
    public void Root_toc_generation_works(EmbeddedFile embeddedFile)
    {
        var baselineFolder = BaselineFolder.BuildOutputFolder(TestContext, embeddedFile);
        var tocFile = baselineFolder.GetBaselineFile(embeddedFile.FileName);

        var typeProvider = new ResourceTypeProvider(new AzTypeLoader());
        var mdGenerator = new MarkdownGenerator(typeProvider);

        var groupedTypes = mdGenerator.GetGroupedTypes().Select(x => x.ProviderNamespace).ToList();

        var tocContents = TocGenerator.GenerateRootToc(new(), groupedTypes);

        tocFile.WriteToOutputFolder(tocContents);
        tocFile.ShouldHaveExpectedValue();
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Bicep.Types.Az;
using Azure.Deployments.Testing.Utilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemplateRefGenerator;

namespace TemplateRefGenerator.Tests;
[TestClass]
public class ChangeLogGeneratorTests
{
    [NotNull]
    public TestContext? TestContext { get; set; }

    [TestMethod]
    [TestCategory(BaselineHelper.BaselineTestCategory)]
    [EmbeddedFilesTestData(@"^Files/changelog/.*/.*\.md$")]
    public void Changelog_generation_works(EmbeddedFile embeddedFile)
    {
        var baselineFolder = BaselineFolder.BuildOutputFolder(TestContext, embeddedFile);
        var markdownFile = baselineFolder.GetBaselineFile(embeddedFile.FileName);

        var streamPathParts = embeddedFile.StreamPath.Split('/').Skip(2).ToArray();
        var providerNamespace = streamPathParts[0];
        var unqualifiedResourceType = string.Join('/', streamPathParts[1..])[..^".md".Length];

        var typeProvider = new ResourceTypeProvider(new AzTypeLoader());
        var mdGenerator = new MarkdownGenerator(typeProvider);
        var changelogGenerator = new ChangelogGenerator(typeProvider);

        var groupedTypes = mdGenerator.GetGroupedTypes()
            .First(x => StringComparer.OrdinalIgnoreCase.Equals(x.ProviderNamespace, providerNamespace));

        var changelog = changelogGenerator.GetChanges(groupedTypes);
        var resourceTypeChange = changelog.ResourceTypeChanges.First(x => StringComparer.OrdinalIgnoreCase.Equals(x.ResourceType, $"{providerNamespace}/{unqualifiedResourceType}"));

        var markdown = ChangelogGenerator.GenerateChangeLog(resourceTypeChange);

        markdownFile.WriteToOutputFolder(markdown);
        markdownFile.ShouldHaveExpectedValue();
    }

    [TestMethod]
    [TestCategory(BaselineHelper.BaselineTestCategory)]
    [EmbeddedFilesTestData(@"^Files/changelog-summary/.*/.*\.md$")]
    public void Changelog_summary_generation_works(EmbeddedFile embeddedFile)
    {
        var baselineFolder = BaselineFolder.BuildOutputFolder(TestContext, embeddedFile);
        var markdownFile = baselineFolder.GetBaselineFile(embeddedFile.FileName);

        var streamPathParts = embeddedFile.StreamPath.Split('/').Skip(2).ToArray();
        var providerNamespace = streamPathParts[0];

        var typeProvider = new ResourceTypeProvider(new AzTypeLoader());
        var mdGenerator = new MarkdownGenerator(typeProvider);
        var changelogGenerator = new ChangelogGenerator(typeProvider);

        var groupedTypes = mdGenerator.GetGroupedTypes()
            .First(x => StringComparer.OrdinalIgnoreCase.Equals(x.ProviderNamespace, providerNamespace));

        var changelog = changelogGenerator.GetChanges(groupedTypes);

        var markdown = ChangelogGenerator.GenerateSummaryChangeLog(changelog);

        markdownFile.WriteToOutputFolder(markdown);
        markdownFile.ShouldHaveExpectedValue();
    }
}
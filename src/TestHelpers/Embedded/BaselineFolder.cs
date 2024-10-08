// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelpers;
public record BaselineFolder(
    TestContext TestContext,
    Assembly StreamAssembly,
    string OutputFolderPath,
    string StreamFolderPath,
    ImmutableDictionary<string, BaselineFile> Files)
{
    public static BaselineFolder BuildOutputFolder(TestContext testContext, EmbeddedFile sourceFile)
        => BuildOutputFolder(testContext, sourceFile.Assembly, sourceFile.StreamPath.Substring(0, sourceFile.StreamPath.LastIndexOf('/')), copyFilesToOutput: true);

    public static BaselineFolder BuildOutputFolder(TestContext testContext, Assembly sourceAssembly, string streamFolderPath, bool copyFilesToOutput = false)
    {
        streamFolderPath.Should().NotContain("\\");
        streamFolderPath.Should().NotEndWith("/");

        var outputDirectory = FileHelper.GetUniqueTestOutputPath(testContext);

        var baselineFiles = new Dictionary<string, BaselineFile>();
        foreach (var streamPath in sourceAssembly.GetManifestResourceNames()
            .Where(file => file.StartsWith(streamFolderPath + "/", StringComparison.Ordinal)))
        {
            var relativePath = streamPath.Substring(streamFolderPath.Length).TrimStart('/');
            var filePath = BaselineHelper.CombinePaths(outputDirectory, relativePath);

            var baselineFile = new BaselineFile(
                testContext,
                new EmbeddedFile(sourceAssembly, streamPath),
                filePath);

            if (copyFilesToOutput && baselineFile.EmbeddedFile.Contents is { } contents)
            {
                baselineFile.WriteToOutputFolder(contents);
            }

            baselineFiles[relativePath] = baselineFile;
        }

        return new(
            testContext,
            sourceAssembly,
            outputDirectory,
            streamFolderPath,
            baselineFiles.ToImmutableDictionary());
    }

    public BaselineFile? TryGetFile(string relativePath)
        => Files.TryGetValue(relativePath, out var file) ? file : null;

    public BaselineFile GetBaselineFile(string relativePath)
    {
        if (TryGetFile(relativePath) is { } baselineFile)
        {
            return baselineFile;
        }

        var embeddedFile = new EmbeddedFile(
            this.StreamAssembly,
            $"{this.StreamFolderPath}/{relativePath}");

        return new(this.TestContext, embeddedFile, BaselineHelper.CombinePaths(OutputFolderPath, relativePath));
    }
}

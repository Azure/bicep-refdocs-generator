// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace TestHelpers;
public record BaselineFile(
    TestContext TestContext,
    EmbeddedFile EmbeddedFile,
    string OutputFilePath)
{
    public string ReadFromOutputFolder() => File.ReadAllText(OutputFilePath);

    public void WriteToOutputFolder(string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(OutputFilePath)!);
        File.WriteAllText(OutputFilePath, contents);
    }

    public void ShouldHaveExpectedValue()
    {
        this.ReadFromOutputFolder().Should().EqualWithLineByLineDiffOutput(
            EmbeddedFile.Assembly,
            TestContext,
            EmbeddedFile.Contents ?? "",
            expectedLocation: EmbeddedFile.RelativeSourcePath,
            actualLocation: OutputFilePath);
    }

    public void ShouldHaveExpectedJsonValue()
    {
        JToken.Parse(this.ReadFromOutputFolder()).Should().EqualWithJsonDiffOutput(
            EmbeddedFile.Assembly,
            TestContext,
            EmbeddedFile.Contents is {} ? JToken.Parse(EmbeddedFile.Contents) : JValue.CreateNull(),
            expectedLocation: EmbeddedFile.RelativeSourcePath,
            actualLocation: OutputFilePath);
    }
}

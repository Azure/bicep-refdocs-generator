// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelpers;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class EmbeddedFilesTestDataAttribute : Attribute, ITestDataSource
{
    public EmbeddedFilesTestDataAttribute(string regexFilter)
    {
        RegexFilter = regexFilter;
    }

    public string RegexFilter { get; }

    public string? SourceAssembly { get; set; }

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var assembly = SourceAssembly is null ? methodInfo.DeclaringType!.Assembly : Assembly.Load(SourceAssembly);

        var files = EmbeddedFile.LoadAll(assembly, new Regex(RegexFilter));
        files.Should().NotBeEmpty($"Expected filter {RegexFilter} to match at least 1 file");

        return files.Select(x => new object[] { x });
    }

    public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        var file = (data![0] as EmbeddedFile)!;

        return $"{methodInfo.Name} ({file.StreamPath})";
    }
}

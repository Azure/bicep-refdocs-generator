// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelpers;
public static class StringAssertionsExtensions
{
    private static Regex NewLineRegex { get; } = new Regex("(\r\n|\r|\n)");

    private static string EscapeWhitespace(string input)
        => input
        .Replace("\r", "\\r")
        .Replace("\n", "\\n")
        .Replace("\t", "\\t");

    private static string GetDiffMarker(ChangeType type)
        => type switch
        {
            ChangeType.Inserted => "++",
            ChangeType.Modified => "//",
            ChangeType.Deleted => "--",
            _ => "  ",
        };

    public static AndConstraint<StringAssertions> EqualWithLineByLineDiffOutput(this StringAssertions instance, Assembly sourceAssembly, TestContext testContext, string expected, string expectedLocation, string actualLocation, string because = "", params object[] becauseArgs)
    {
        const int truncate = 100;
        var diff = InlineDiffBuilder.Diff(expected, instance.Subject, ignoreWhiteSpace: false, ignoreCase: false);

        var lineLogs = diff.Lines
            .Where(line => line.Type != ChangeType.Unchanged)
            .Select(line => $"[{line.Position}] {GetDiffMarker(line.Type)} {EscapeWhitespace(line.Text)}")
            .Take(truncate);

        if (lineLogs.Count() >= truncate)
        {
            lineLogs = lineLogs.Concat(new[] { "...truncated..." });
        }

        var testPassed = !diff.HasDifferences;
        var isBaselineUpdate = !testPassed && BaselineHelper.ShouldSetBaseline(testContext);
        if (isBaselineUpdate)
        {
            BaselineHelper.SetBaseline(actualLocation, expectedLocation);
        }

        var fullActualLocation = BaselineHelper.TryGetAbsolutePathRelativeToRepoRoot(actualLocation);
        var fullExpectedLocation = BaselineHelper.TryGetAbsolutePathRelativeToRepoRoot(expectedLocation);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(testPassed)
            .FailWith(
                BaselineHelper.GetAssertionFormatString(isBaselineUpdate, canUpdateBaselines: fullActualLocation is not null && fullExpectedLocation is not null),
                string.Join("\n", lineLogs),
                fullActualLocation,
                fullExpectedLocation,
                sourceAssembly.Location);

        return new AndConstraint<StringAssertions>(instance);
    }

    public static AndConstraint<StringAssertions> BeEquivalentToIgnoringNewlines(this StringAssertions instance, string expected, string because = "", params object[] becauseArgs)
    {
        var normalizedActual = ReplaceNewlines(instance.Subject, "\n");
        var normalizedExpected = ReplaceNewlines(expected, "\n");

        normalizedActual.Should().BeEquivalentTo(normalizedExpected, because, becauseArgs);

        return new AndConstraint<StringAssertions>(instance);
    }

    public static AndConstraint<StringAssertions> EqualIgnoringNewlines(this StringAssertions instance, string expected, string because = "", params object[] becauseArgs)
    {
        var normalizedActual = ReplaceNewlines(instance.Subject, "\n");
        var normalizedExpected = ReplaceNewlines(expected, "\n");

        normalizedActual.Should().Be(normalizedExpected, because, becauseArgs);

        return new AndConstraint<StringAssertions>(instance);
    }

    public static AndConstraint<StringAssertions> ContainIgnoringNewlines(this StringAssertions instance, string expected)
    {
        var normalizedActual = ReplaceNewlines(instance.Subject, "\n");
        var normalizedExpected = ReplaceNewlines(expected, "\n");

        normalizedActual.Should().Contain(normalizedExpected);

        return new AndConstraint<StringAssertions>(instance);
    }

    public static AndConstraint<StringAssertions> BeEquivalentToPath(this StringAssertions instance, string expected, string because = "", params object[] becauseArgs)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            instance.Subject.Should().BeEquivalentTo(expected, because, becauseArgs);
        }
        else
        {
            instance.Subject.Should().Be(expected, because, becauseArgs);
        }

        return new AndConstraint<StringAssertions>(instance);
    }

    public static AndConstraint<StringAssertions> HaveLengthLessThanOrEqualTo(this StringAssertions instance, int maxLength, string because = "", params object[] becauseArgs)
    {
        int length = instance.Subject.Length;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(length <= maxLength)
            .FailWith("Expected {0} to have length less than or equal to {1}, but it has length {2}", instance.Subject, maxLength, length);

        return new AndConstraint<StringAssertions>(instance);
    }

    public static string ReplaceNewlines(string value, string newlineReplacement) =>
            NewLineRegex.Replace(value, newlineReplacement);
}

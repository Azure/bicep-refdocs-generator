// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using JsonDiffPatchDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace TestHelpers;
public static class JTokenAssertionsExtensions
{
    public static JTokenAssertions Should(this JToken? instance)
    {
        return new JTokenAssertions(instance);
    }

    private static string? GetJsonDiff(JToken? first, JToken? second)
    {
        const int truncate = 100;
        var diff = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Simple }).Diff(first, second);
        if (diff is null)
        {
            return null;
        }

        // JsonDiffPatch.Diff returns null if there are no diffs
        var lineLogs = diff.ToString().Split('\n').Take(truncate);

        if (lineLogs.Count() >= truncate)
        {
            lineLogs = lineLogs.Concat(new[] { "...truncated..." });
        }

        return string.Join("\n", lineLogs);
    }

    public static AndConstraint<JTokenAssertions> EqualWithJsonDiffOutput(this JTokenAssertions instance, Assembly sourceAssembly, TestContext testContext, JToken expected, string expectedLocation, string actualLocation, string because = "", params object[] becauseArgs)
    {
        var jsonDiff = GetJsonDiff(instance.Subject, expected);

        var testPassed = jsonDiff is null;
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
                jsonDiff,
                fullActualLocation,
                fullExpectedLocation,
                sourceAssembly.Location);

        return new AndConstraint<JTokenAssertions>(instance);
    }

    public static AndConstraint<JTokenAssertions> DeepEqual(this JTokenAssertions instance, JToken expected, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(JToken.DeepEquals(instance.Subject, expected))
            .FailWith("Expected {0} but got {1}. Differences: {2}", expected.ToString(), instance.Subject?.ToString(), GetJsonDiff(instance.Subject, expected));

        return new AndConstraint<JTokenAssertions>(instance);
    }

    public static AndConstraint<JTokenAssertions> HaveValueAtPath(this JTokenAssertions instance, string jtokenPath, JToken expected, string because = "", params object[] becauseArgs)
    {
        var valueAtPath = instance.Subject?.SelectToken(jtokenPath);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(valueAtPath is not null)
            .FailWith("Expected value at path {0} to be {1}{reason} but it was null. Original JSON: {2}", jtokenPath, expected.ToString(), instance.Subject?.ToString());

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(JToken.DeepEquals(valueAtPath, expected))
            .FailWith("Expected value at path {0} to be {1}{reason} but it was {2}", jtokenPath, expected.ToString(), valueAtPath?.ToString());

        return new AndConstraint<JTokenAssertions>(instance);
    }

    public static AndConstraint<JTokenAssertions> NotHaveValueAtPath(this JTokenAssertions instance, string jtokenPath, string because = "", params object[] becauseArgs)
    {
        var valueAtPath = instance.Subject?.SelectToken(jtokenPath);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(valueAtPath is null)
            .FailWith("Expected value at path {0} to be null{reason}, but it was {1}", jtokenPath, valueAtPath);

        return new AndConstraint<JTokenAssertions>(instance);
    }

    public static AndConstraint<JTokenAssertions> NotHaveValue(this JTokenAssertions instance, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(instance.Subject is null)
            .FailWith("Expected value to be null{reason}, but it was {0}", instance.Subject?.ToString());

        return new AndConstraint<JTokenAssertions>(instance);
    }
}

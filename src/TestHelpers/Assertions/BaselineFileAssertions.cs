// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.IO;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace TestHelpers;
public class BaselineFileAssertions : ReferenceTypeAssertions<BaselineFile, BaselineFileAssertions>
{
    public BaselineFileAssertions(BaselineFile instance)
        : base(instance)
    {
    }

    protected override string Identifier => "baselineFile";
}

public static class BaselineFileAssertionsExtensions
{
    public static BaselineFileAssertions Should(this BaselineFile instance)
    {
        return new BaselineFileAssertions(instance);
    }

    public static AndConstraint<BaselineFileAssertions> ExistWithBaselineUpdateInfo(this BaselineFileAssertions instance, string because = "", params object[] becauseArgs)
    {
        var testPassed = File.Exists(instance.Subject.OutputFilePath);
        var isBaselineUpdate = !testPassed && BaselineHelper.ShouldSetBaseline(instance.Subject.TestContext);
        if (isBaselineUpdate)
        {
            BaselineHelper.RemoveBaseline(instance.Subject.EmbeddedFile.RelativeSourcePath);
        }

        var fullLocation = BaselineHelper.TryGetAbsolutePathRelativeToRepoRoot(instance.Subject.EmbeddedFile.RelativeSourcePath);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(testPassed)
            .FailWith(
                BaselineHelper.GetDeleteAssertionFormatString(isBaselineUpdate, canUpdateBaselines: fullLocation is not null),
                fullLocation,
                instance.Subject.EmbeddedFile.Assembly.Location);

        return new AndConstraint<BaselineFileAssertions>(instance);
    }
}

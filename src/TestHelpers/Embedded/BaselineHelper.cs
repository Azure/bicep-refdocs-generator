// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelpers;
public static class BaselineHelper
{
    private static readonly string? RepoRoot = TryGetRepoRoot();
    private const string SetBaseLineSettingName = "SetBaseLine";
    public const string BaselineTestCategory = "Baseline";

    public static bool ShouldSetBaseline(TestContext testContext) =>
        testContext.Properties.Contains(SetBaseLineSettingName) && string.Equals(testContext.Properties[SetBaseLineSettingName] as string, bool.TrueString, StringComparison.OrdinalIgnoreCase);

    public static void SetBaseline(string actualLocation, string expectedLocation)
    {
        if (TryGetAbsolutePathRelativeToRepoRoot(actualLocation) is not { } fullActualLocation ||
            TryGetAbsolutePathRelativeToRepoRoot(expectedLocation) is not { } fullExpectedLocation)
        {
            throw new InvalidOperationException($"Baseline update failed: Unable to determine the repo root path from directory {Environment.CurrentDirectory}");
        }

        if (Path.GetDirectoryName(fullExpectedLocation) is { } parentDir &&
            !Directory.Exists(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }

        File.Copy(fullActualLocation, fullExpectedLocation, overwrite: true);
    }

    public static void RemoveBaseline(string expectedLocation)
    {
        if (TryGetAbsolutePathRelativeToRepoRoot(expectedLocation) is not { } fullExpectedLocation)
        {
            throw new InvalidOperationException($"Baseline update failed: Unable to determine the repo root path from directory {Environment.CurrentDirectory}");
        }

        File.Delete(fullExpectedLocation);
    }

    public static string? TryGetAbsolutePathRelativeToRepoRoot(string path)
        => RepoRoot is not null ? CombinePaths(RepoRoot, path) : null;

    public static string CombinePaths(params string[] paths)
        => Path.Combine(paths).Replace('/', Path.DirectorySeparatorChar);


    private static string? TryGetRepoRoot()
    {
        var currentDir = new DirectoryInfo(Environment.CurrentDirectory);

        while (currentDir.Parent is { } parentDir)
        {
            // search upwards for the .git directory. This should only exist at the repository root.
            if (Directory.Exists(Path.Combine(currentDir.FullName, ".git")))
            {
                return currentDir.FullName;
            }

            currentDir = parentDir;
        }

        // on CI systems, there's no guarantee we'll be able to find the .git folder
        return null;
    }

    public static string GetAssertionFormatString(bool isBaselineUpdate, bool canUpdateBaselines)
    {
        var output = new StringBuilder();
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        output.Append(@"
Found diffs between actual and expected:
{0}
");

        if (isBaselineUpdate)
        {
            output.Append(@"
Baseline {2} has been updated.
");
        }
        else if (canUpdateBaselines)
        {
            output.Append(@"
View this diff with:
    git diff --color-words --no-index {2} {1}
");

            if (isWindows)
            {
                output.Append(@"
Overwrite the single baseline:
    xcopy /yq {1} {2}

Overwrite all baselines for this assembly:
    dotnet test {3} --filter ""TestCategory=Baseline"" -- 'TestRunParameters.Parameter(name=""SetBaseLine"", value=""true"")'

Overwrite all baselines:
    .\scripts\UpdateBaselines.ps1
");
            }
            else
            {
                output.Append(@"
Overwrite the single baseline:
    cp {1} {2}

Overwrite all baselines for this assembly:
    dotnet test {3} --filter ""TestCategory=Baseline"" -- 'TestRunParameters.Parameter(name=""SetBaseLine"", value=""true"")'
");
            }
        }
        else
        {
            output.Append(@"
Run the following in the repo root to overwrite all baselines:
    .\scripts\UpdateBaselines.ps1
");
        }

        output.Append(@"
For information on baselines, see https://aka.ms/deployments/wiki?pagePath=/How-Tos/Work-with-Baseline-files
");

        return output.ToString();
    }

    public static string GetDeleteAssertionFormatString(bool isBaselineUpdate, bool canUpdateBaselines)
    {
        var output = new StringBuilder();
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        output.Append(@"
Baseline was not generated in the output directory.
");

        if (isBaselineUpdate)
        {
            output.Append(@"
Baseline {0} has been deleted.
");
        }
        else if (canUpdateBaselines)
        {
            if (isWindows)
            {
                output.Append(@"
Delete the single baseline:
    del {0}

Overwrite all baselines for this assembly:
    dotnet test {1} --filter ""TestCategory=Baseline"" -- 'TestRunParameters.Parameter(name=""SetBaseLine"", value=""true"")'

Overwrite all baselines:
    .\scripts\UpdateBaselines.ps1
");
            }
            else
            {
                output.Append(@"
Delete the single baseline:
    rm {0}

Overwrite all baselines for this assembly:
    dotnet test {1} --filter ""TestCategory=Baseline"" -- 'TestRunParameters.Parameter(name=""SetBaseLine"", value=""true"")'
");
            }
        }
        else
        {
            output.Append(@"
Run the following in the repo root to overwrite all baselines:
    .\scripts\UpdateBaselines.ps1
");
        }

        output.Append(@"
For information on baselines, see https://aka.ms/deployments/wiki?pagePath=/How-Tos/Work-with-Baseline-files
");

        return output.ToString();
    }
}

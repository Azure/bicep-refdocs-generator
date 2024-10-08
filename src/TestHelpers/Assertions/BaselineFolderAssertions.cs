// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.IO;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace TestHelpers;
public class BaselineFolderAssertions : ReferenceTypeAssertions<BaselineFolder, BaselineFolderAssertions>
{
    public BaselineFolderAssertions(BaselineFolder instance)
        : base(instance)
    {
    }

    protected override string Identifier => "baselineFolder";
}

public static class BaselineFolderAssertionsExtensions
{
    public static BaselineFolderAssertions Should(this BaselineFolder instance)
    {
        return new BaselineFolderAssertions(instance);
    }

    public static AndConstraint<BaselineFolderAssertions> ContainExpectedFiles(this BaselineFolderAssertions instance)
    {
        using (new AssertionScope())
        {
            var baselineFolder = instance.Subject;
            foreach (var generatedFile in Directory.GetFiles(baselineFolder.OutputFolderPath, "*.*", SearchOption.AllDirectories))
            {
                generatedFile.Should().StartWith(baselineFolder.OutputFolderPath);
                var relativePath = generatedFile.Substring(baselineFolder.OutputFolderPath.Length + 1)
                    .Replace(Path.DirectorySeparatorChar, '/');

                var baselineFile = baselineFolder.GetBaselineFile(relativePath);
                if (baselineFile.OutputFilePath.EndsWith(".json"))
                {
                    baselineFile.ShouldHaveExpectedJsonValue();
                }
                else
                {
                    baselineFile.ShouldHaveExpectedValue();
                }
            }

            foreach (var baselineFile in baselineFolder.Files.Values)
            {
                baselineFile.Should().ExistWithBaselineUpdateInfo();
            }
        }

        return new AndConstraint<BaselineFolderAssertions>(instance);
    }
}

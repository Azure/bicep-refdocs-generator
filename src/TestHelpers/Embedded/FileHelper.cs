// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelpers;
public static class FileHelper
{
    public static string GetUniqueTestOutputPath(TestContext testContext)
        => Path.Combine(testContext.ResultsDirectory!, Guid.NewGuid().ToString());
}

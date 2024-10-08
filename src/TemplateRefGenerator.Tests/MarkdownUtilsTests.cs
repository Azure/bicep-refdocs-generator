// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemplateRefGenerator;

namespace TemplateRefGenerator.Tests;
[TestClass]
public class MarkdownUtilsTests
{
    [TestMethod]
    public void Markdown_link_generation_works()
    {
        MarkdownUtils.GetLink("2020-01-01", "../../foo.md").Should().Be("[2020-01-01](../../foo.md)");
    }
}
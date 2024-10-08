// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TemplateRefGenerator;

public class MarkdownUtils
{
    public static string GetLink(string text, string destination)
        => $"[{text}]({destination})";
}
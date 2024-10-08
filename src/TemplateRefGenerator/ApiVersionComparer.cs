// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;

namespace TemplateRefGenerator;
public class ApiVersionComparer : IComparer<string>
{
    public static ApiVersionComparer Instance { get; } = new ApiVersionComparer();

    private ApiVersionComparer()
    {
    }

    public int Compare(string? x, string? y)
    {
        if (x == null || y == null || x.Length == y.Length)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(x, y);
        }

        if (x.Length < y.Length)
        {
            var compare = StringComparer.OrdinalIgnoreCase.Compare(x, y.Substring(0, x.Length));

            return compare != 0 ? compare : 1;
        }
        else
        {
            var compare = StringComparer.OrdinalIgnoreCase.Compare(x.Substring(0, y.Length), y);

            return compare != 0 ? compare : -1;
        }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TemplateRefGenerator.MarkdownGenerator;

namespace TemplateRefGenerator;
public class AllVersionsGenerator
{
    private static string GetHeading(PageMetadata metadata, GroupedTypes groupedTypes)
    {
        return $"""
---
title: Azure Resource Manager template reference for {groupedTypes.ProviderNamespace}"
description: Azure Resource Manager template reference for the {groupedTypes.ProviderNamespace} resource provider. Includes all resource types and versions.";
author: {metadata.Author}
ms.service: azure-resource-manager
ms.topic: reference
ms.date: {metadata.Date.ToString("MM/dd/yyyy")}
ms.author: {metadata.MsAuthor}
---
""";
    }

    private static string GetResourceTypesAndVersions(GroupedTypes groupedTypes)
    {
        var sb = new StringBuilder();
        sb.Append($"""
| Types | Versions |
| ----- | --------- |

""");


        foreach (var (type, versions) in groupedTypes.GetApiVersionsByType().OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            var links = new List<string>();
            var unqualifiedType = Utils.GetUnqualifiedType(type);

            foreach (var version in versions.OrderBy(x => x, ApiVersionComparer.Instance))
            {
                var mdPath = $"./{version}/{unqualifiedType}.md".ToLowerInvariant();
                links.Add($"[{version}]({mdPath})");
            }

            sb.Append($"""
| {type} | {string.Join("<br>", links)} |

""");
        }

        return sb.ToString();
    }

    public static string GenerateMarkdown(PageMetadata pageMetadata, GroupedTypes groupedTypes)
    {
        return $"""
{GetHeading(pageMetadata, groupedTypes)}
# {groupedTypes.ProviderNamespace} resource types

This article lists the available versions for each resource type.

For a list of changes in each API version, see [change log](./change-log/summary.md)

## Resource types and versions

{GetResourceTypesAndVersions(groupedTypes)}
""";
    }
}
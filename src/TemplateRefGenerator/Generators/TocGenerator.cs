// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateRefGenerator;
public static class TocGenerator
{
    private record TocItem(
        string Name,
        List<TocItem> Items,
        string? Href = null,
        string? DisplayName = null);

    private static string GetIndent(int indentLevel)
        => indentLevel > 0 ? string.Join("", Enumerable.Repeat("  ", indentLevel)) : "";

    public static string GenerateReferenceToc(MarkdownGenerator.GroupedTypes groupedTypes)
    {
        TocItem latestVersionToc = new("<placeholder>", new());
        foreach (var (resourceType, apiVersion) in groupedTypes.GetLatestApiVersionByType().OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            var parentToc = latestVersionToc;
            var segments = resourceType.Split('/').Skip(1).ToArray();
            foreach (var parentSegment in segments[0..^1])
            {
                if (parentToc.Items.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, parentSegment)) is not {} nextParentToc)
                {
                    nextParentToc = new(parentSegment, new());
                    parentToc.Items.Add(nextParentToc);
                }

                parentToc = nextParentToc;
            }

            var mdPath = $"./{string.Join('/', segments)}.md".ToLowerInvariant();
            TocItem childToc = new(segments.Last(), new(), mdPath);
            parentToc.Items.Add(childToc);
        }

        TocItem apiVersionsToc = new("(Api versions)", new());
        foreach (var (apiVersion, resourceTypes) in groupedTypes.ResourceTypeByApiVersion.OrderByDescending(x => x.Key, ApiVersionComparer.Instance))
        {
            TocItem apiVersionToc = new(apiVersion, new());
            apiVersionsToc.Items.Add(apiVersionToc);

            foreach (var resourceType in resourceTypes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var parentToc = apiVersionToc;
                var segments = resourceType.Split('/').Skip(1).ToArray();
                foreach (var parentSegment in segments[0..^1])
                {
                    if (parentToc.Items.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, parentSegment)) is not {} nextParentToc)
                    {
                        nextParentToc = new(parentSegment, new());
                        parentToc.Items.Add(nextParentToc);
                    }

                    parentToc = nextParentToc;
                }

                var mdPath = $"./{apiVersion}/{string.Join('/', segments)}.md".ToLowerInvariant();
                TocItem childToc = new(segments.Last(), new(), mdPath);
                parentToc.Items.Add(childToc);
            }
        }

        var sb = new StringBuilder();
        foreach (var toc in latestVersionToc.Items)
        {
            Print(toc, sb, 0);
        }
        Print(apiVersionsToc, sb, 0);
        return sb.ToString();
    }

    public static string GenerateChangeLogToc(MarkdownGenerator.GroupedTypes groupedTypes)
    {
        TocItem latestVersionToc = new("<placeholder>", new());
        foreach (var (resourceType, _) in groupedTypes.GetApiVersionsByType().OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            var parentToc = latestVersionToc;
            var segments = resourceType.Split('/').Skip(1).ToArray();
            foreach (var parentSegment in segments[0..^1])
            {
                if (parentToc.Items.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, parentSegment)) is not {} nextParentToc)
                {
                    nextParentToc = new(parentSegment, new());
                    parentToc.Items.Add(nextParentToc);
                }

                parentToc = nextParentToc;
            }

            var mdPath = $"./{string.Join('/', segments)}.md".ToLowerInvariant();
            TocItem childToc = new(segments.Last(), new(), mdPath);
            parentToc.Items.Add(childToc);
        }

        var sb = new StringBuilder();
        foreach (var toc in latestVersionToc.Items)
        {
            Print(toc, sb, 0);
        }
        return sb.ToString();
    }

    public static string GenerateRootToc(ConfigLoader configLoader, IReadOnlyList<string> providerNamespaces)
    {
        var sb = new StringBuilder();
        var config = configLoader.GetConfiguration();
        var titleMapping = config.TocTitleMapping.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        sb.Append($"""
- name: Overview
  items:
  - name: Define resources
    href: index.md

""");

        sb.Append($"""
- name: Change log
  items:

""");
        foreach (var provider in providerNamespaces)
        {
            var tocTitle = titleMapping.TryGetValue(provider, out var title) ? title : provider;

        sb.Append($"""
  - name: {tocTitle}
    href: ./{provider}/change-log/toc.yml

""");
        }

        sb.Append($"""
- name: Reference
  expanded: true
  items:

""");
        foreach (var provider in providerNamespaces)
        {
            var tocTitle = titleMapping.TryGetValue(provider, out var title) ? title : provider;

        sb.Append($"""
  - name: {tocTitle}
    href: ./{provider}/toc.yml

""");
        }

        return sb.ToString();
    }

    private static void Print(TocItem toc, StringBuilder sb, int indentLevel)
    {
        var indent = GetIndent(indentLevel);

        sb.AppendLine($"{indent}- name: {toc.Name}");
        if (toc.DisplayName is {})
        {
            sb.AppendLine($"{indent}  displayName: {toc.DisplayName}");
        }
        if (toc.Href is {})
        {
            sb.AppendLine($"{indent}  href: {toc.Href}");
        }
        if (toc.Items.Any())
        {
            sb.AppendLine($"{indent}  items:");
            foreach (var child in toc.Items)
            {
                Print(child, sb, indentLevel + 1);
            }
        }
    }
}
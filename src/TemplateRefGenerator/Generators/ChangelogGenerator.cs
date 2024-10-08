// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Azure.Bicep.Types.Concrete;
using static TemplateRefGenerator.MarkdownGenerator;

namespace TemplateRefGenerator;
public class ChangelogGenerator
{
    private readonly ResourceTypeProvider typeProvider;

    public ChangelogGenerator(ResourceTypeProvider typeProvider)
    {
        this.typeProvider = typeProvider;
    }

    public record ProviderChangeLog(
        string ProviderNamespace,
        ImmutableArray<ResourceTypeChangeLog> ResourceTypeChanges);

    public record ResourceTypeChangeLog(
        string ResourceType,
        ImmutableArray<VersionedChangeLog> Changes);

    public record VersionedChangeLog(
        string Version,
        ResourceType? PrevType,
        ResourceType NewType,
        ImmutableArray<NamedTypeChangeLog> Changes);

    public record NamedTypeChangeLog(
        string Name,
        TypeBase? Before,
        TypeBase? After);

    public ProviderChangeLog GetChanges(GroupedTypes groupedTypes)
    {
        var apiVersionsByType = groupedTypes.GetApiVersionsByType();

        var allChanges = new List<ResourceTypeChangeLog>();
        foreach (var (typeName, apiVersions) in apiVersionsByType)
        {
            var changes = GetChangesForResourceType(typeName, apiVersions);
            allChanges.Add(changes);
        }

        return new(groupedTypes.ProviderNamespace, [.. allChanges]);
    }

    private ResourceTypeChangeLog GetChangesForResourceType(string typeName, IReadOnlyList<string> apiVersions)
    {
        List<VersionedChangeLog> versionChanges = new();
        var apiVersionsOrdered = apiVersions.OrderBy(x => x, ApiVersionComparer.Instance).ToImmutableArray();
        for (var i = 0; i < apiVersionsOrdered.Length; i++)
        {
            var prevVersion = i > 0 ? apiVersionsOrdered[i - 1] : null;
            ResourceType? prevType = null;
            var newVersion = apiVersionsOrdered[i];

            var prevNamedTypes = ImmutableDictionary<string, TypeBase>.Empty;
            if (prevVersion is {})
            {
                prevType = typeProvider.Get(typeName, prevVersion);
                prevNamedTypes = GetNamedTypes(prevType);
            }

            var newType = typeProvider.Get(typeName, newVersion);
            var newNameTypes = GetNamedTypes(newType);

            var allNames = newNameTypes.Keys.Union(prevNamedTypes.Keys, StringComparer.OrdinalIgnoreCase).Distinct(StringComparer.OrdinalIgnoreCase);
            var changes = allNames
                .Select(x => new NamedTypeChangeLog(
                    x,
                    prevNamedTypes.TryGetValue(x, out var before) ? before : null,
                    newNameTypes.TryGetValue(x, out var after) ? after : null))
                .ToImmutableArray();

            versionChanges.Add(new(newVersion, prevType, newType, changes));
        }

        return new(typeName, [.. versionChanges]);
    }

    private static string GetHeading(PageMetadata metadata, ResourceTypeChangeLog changeLog)
    {
        return $"""
---
title: API change log for {changeLog.ResourceType}
description: Describes changes between API versions for {changeLog.ResourceType}.
author: {metadata.Author}
ms.service: azure-resource-manager
ms.topic: reference
ms.date: {metadata.Date.ToString("MM/dd/yyyy")}
ms.author: {metadata.MsAuthor}
---
""";
    }

    public static string GenerateChangeLog(PageMetadata metadata, ResourceTypeChangeLog changeLog)
    {
        return $"""
{GetHeading(metadata, changeLog)}
# API version change log for deployment of {changeLog.ResourceType}

This article describes the properties that changed in each API version for [{changeLog.ResourceType.ToLowerInvariant()}](~/{changeLog.ResourceType.ToLowerInvariant()}.md). It only covers properties that are available during deployments.

{GetVersionChanges(changeLog)}
""";
    }

    private static string GetSummaryHeading(PageMetadata metadata, ProviderChangeLog changeLog)
    {
        return $"""
---
title: API change log for {changeLog.ProviderNamespace} deployment resource types
description: Describes changes between API versions for {changeLog.ProviderNamespace}.
author: {metadata.Author}
ms.service: azure-resource-manager
ms.topic: reference
ms.date: {metadata.Date.ToString("MM/dd/yyyy")}
ms.author: {metadata.MsAuthor}
---
""";
    }

    public static string GenerateSummaryChangeLog(PageMetadata metadata, ProviderChangeLog changeLog)
    {
        return $"""
{GetSummaryHeading(metadata, changeLog)}

# Change log for deployment of {changeLog.ProviderNamespace} resource types

This article summarizes the changes between API versions for {changeLog.ProviderNamespace}. It only covers resource types that are available during deployment. For details about the properties that changed, select the resource type

{GetSummaryVersionChanges(changeLog)}
""";
    }

    private static string GetVersionChanges(ResourceTypeChangeLog resourceTypeChange)
    {
        var sb = new StringBuilder();
        foreach (var versionChange in resourceTypeChange.Changes.OrderByDescending(x => x.Version, ApiVersionComparer.Instance))
        {
            if (versionChange.PrevType is null)
            {
                sb.Append($"""
## {versionChange.Version}

Oldest version tracked in change log

""");
            }
            else
            {
                sb.Append($"""
## {versionChange.Version}

{GetPropertyChanges(resourceTypeChange, versionChange, versionChange.Changes)}

""");
            }
        }

        return sb.ToString();
    }

    private static string GetPropertyChanges(ResourceTypeChangeLog resourceTypeChange, VersionedChangeLog versionedChange, ImmutableArray<NamedTypeChangeLog> changes)
    {
        var sb = new StringBuilder();
        var hasChanges = false;
        var added = changes.Where(x => x.Before is null).ToArray();
        if (added.Any())
        {
            hasChanges = true;
            sb.Append($"""
Added:


""");

            foreach (var entry in added.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                sb.Append($"""
* {GetTypeLink(resourceTypeChange.ResourceType, versionedChange.Version, entry.Name)}

""");
            }

            sb.AppendLine();
        }

        var removed = changes.Where(x => x.After is null).ToArray();
        if (removed.Any())
        {
            hasChanges = true;
            sb.Append($"""
Removed:


""");

            foreach (var entry in removed.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                sb.Append($"""
* {GetTypeLink(resourceTypeChange.ResourceType, versionedChange.Version, entry.Name)}

""");
            }

            sb.AppendLine();
        }

        var updates = GetUpdates(resourceTypeChange, versionedChange, changes).ToArray();
        if (updates.Any())
        {
            hasChanges = true;
            sb.Append($"""
Updated:


""");

            foreach (var update in updates)
            {
                sb.Append($"""
{update}

""");
            }

            sb.AppendLine();
        }

        if (!hasChanges)
        {
            sb.Append($"""
No properties added, updated or removed.

""");
        }

        return sb.ToString();
    }

    private static string GetTypeLink(string resourceType, string apiVersion, string typeName)
    {
        var providerNamespace = Utils.GetProviderNamespace(resourceType);
        var unqualifiedResourceType = Utils.GetUnqualifiedType(resourceType);

        var mdFilePath = $"~/{providerNamespace}/{apiVersion}/{unqualifiedResourceType}.md".ToLowerInvariant();

        return $"[{typeName}]({mdFilePath}#{typeName.ToLowerInvariant()})";
    }

    private static IEnumerable<(string? prevName, string? newName)> GetObjectPropertyChanges(ResourceTypeChangeLog resourceTypeChange, VersionedChangeLog versionedChange, IReadOnlyDictionary<string, ObjectTypeProperty> before, IReadOnlyDictionary<string, ObjectTypeProperty> after)
    {
        var resourceType = resourceTypeChange.ResourceType;
        var version = versionedChange.Version;

        foreach (var (propName, value) in after.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Where(x => !before.ContainsKey(x.Key)))
        {
            yield return (null, propName);
        }

        foreach (var (propName, value) in before.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Where(x => !after.ContainsKey(x.Key)))
        {
            yield return (propName, null);
        }
    }

    private static IEnumerable<string> GetUpdates(ResourceTypeChangeLog resourceTypeChange, VersionedChangeLog versionedChange, IEnumerable<NamedTypeChangeLog> changes)
    {
        var resourceType = resourceTypeChange.ResourceType;
        var version = versionedChange.Version;

        foreach (var change in changes.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (change.Before is null || change.After is null)
            {
                // ignore additions & deletions - this is just to log updates
                continue;
            }

            switch ((change.Before, change.After))
            {
                case (ObjectType beforeObj, ObjectType afterObj):
                    foreach (var (beforeProp, afterProp) in GetObjectPropertyChanges(resourceTypeChange, versionedChange, beforeObj.Properties, afterObj.Properties))
                    {
                        if (beforeProp is null && afterProp is {})
                        {
                            yield return $"""
* {GetTypeLink(resourceType, version, change.Name)}: Added property '{afterProp}'
""";
                        }
                        if (beforeProp is {} && afterProp is null)
                        {
                            yield return $"""
* {GetTypeLink(resourceType, version, change.Name)}: Removed property '{beforeProp}'
""";
                        }
                    }
                    break;
                case (DiscriminatedObjectType beforeDisc, DiscriminatedObjectType afterDisc):
                    foreach (var (beforeProp, afterProp) in GetObjectPropertyChanges(resourceTypeChange, versionedChange, beforeDisc.BaseProperties, afterDisc.BaseProperties))
                    {
                        if (beforeProp is null && afterProp is {})
                        {
                            yield return $"""
* {GetTypeLink(resourceType, version, change.Name)}: Added property '{afterProp}'
""";
                        }
                        if (beforeProp is {} && afterProp is null)
                        {
                            yield return $"""
* {GetTypeLink(resourceType, version, change.Name)}: Removed property '{afterProp}'
""";
                        }
                    }
                    break;
                case ({} beforeType, {} afterType):
                    yield return $"""
* {GetTypeLink(resourceType, version, change.Name)}: Changed type from {beforeType.GetType()} to {afterType.GetType()}
""";
                    break;
            }
        }
    }

    private static ImmutableDictionary<string, TypeBase> GetNamedTypes(ResourceType resourceType)
    {
        var referencedTypes = new HashSet<TypeBase>();
        GetReferencedTypes(resourceType.Body.Type, referencedTypes);

        var namedTypes = new Dictionary<string, TypeBase>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in referencedTypes)
        {
            switch (type)
            {
                case ObjectType objectType:
                    namedTypes[objectType.Name] = objectType;
                    break;
                case DiscriminatedObjectType discType:
                    namedTypes[discType.Name] = discType;
                    break;
            }
        }

        return namedTypes.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static void GetReferencedTypes(TypeBase type, HashSet<TypeBase> types)
    {
        if (types.Contains(type))
        {
            // avoid cycles
            return;
        }

        types.Add(type);

        switch (type)
        {
            case ArrayType arrayType:
                types.Add(arrayType.ItemType.Type);
                break;
            case ObjectType objectType:
                foreach (var property in objectType.Properties.Values)
                {
                    GetReferencedTypes(property.Type.Type, types);
                }
                if (objectType.AdditionalProperties is {} addType)
                {
                    GetReferencedTypes(addType.Type, types);
                }
                break;
            case DiscriminatedObjectType discType:
                foreach (var property in discType.BaseProperties.Values)
                {
                    GetReferencedTypes(property.Type.Type, types);
                }
                foreach (var element in discType.Elements.Values)
                {
                    GetReferencedTypes(element.Type, types);
                }
                break;
            case UnionType unionType:
                foreach (var element in unionType.Elements)
                {
                    GetReferencedTypes(element.Type, types);
                }
                break;
        }
    }

    private static string GetResourceTypeLink(string resourceType, string apiVersion)
    {
        var providerNamespace = Utils.GetProviderNamespace(resourceType);
        var unqualifiedResourceType = Utils.GetUnqualifiedType(resourceType);

        return MarkdownUtils.GetLink(unqualifiedResourceType, $"~/{providerNamespace}/change-log/{unqualifiedResourceType}.md#{apiVersion}".ToLowerInvariant());
    }

    private static string GetSummaryVersionChanges(ProviderChangeLog changeLog)
    {
        var sb = new StringBuilder();
        var changesByVersion = changeLog.ResourceTypeChanges
            .SelectMany(x => x.Changes.Select(y => (resourceType: x, versioned: y)))
            .GroupBy(x => x.versioned.Version);

        foreach (var versionChanges in changesByVersion.OrderBy(x => x.Key, ApiVersionComparer.Instance))
        {
            var apiVersion = versionChanges.Key;
            var added = new List<string>();
            var updated = new List<string>();

            foreach (var resourceTypeChanges in versionChanges.OrderBy(x => x.resourceType.ResourceType, StringComparer.OrdinalIgnoreCase))
            {
                var resourceTypeChange = resourceTypeChanges.resourceType;;
                var versionedChange = resourceTypeChanges.versioned;

                if (versionedChange.PrevType is null)
                {
                    added.Add(resourceTypeChange.ResourceType);
                }
                else if (GetUpdates(resourceTypeChange, versionedChange, versionedChange.Changes).Any())
                {
                    updated.Add(resourceTypeChange.ResourceType);
                }
            }

                sb.Append($"""

## {apiVersion}


""");

            if (!added.Any() && !updated.Any())
            {
                sb.Append($"""
No new or updated resource types.

""");
                continue;
            }

            sb.Append($"""
> [!div class="mx-tableFixed"]
> | New | Updated |
> |-----|---------|

""");

            for (var i = 0; i < Math.Max(added.Count, updated.Count); i++)
            {
                var addedLink = i < added.Count ? GetResourceTypeLink(added[i], apiVersion) : "";
                var updatedLink = i < updated.Count ? GetResourceTypeLink(updated[i], apiVersion) : "";

                sb.Append($"""
> | {addedLink} | {updatedLink} |

""");
            }
        }

        sb.AppendLine();

        return sb.ToString();
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Azure.Bicep.Types.Concrete;
using static TemplateRefGenerator.RemarksLoader;

namespace TemplateRefGenerator;
public class MarkdownGenerator
{
    private record StaticTypeReference(TypeBase Type) : ITypeReference;

    private readonly ResourceTypeProvider typeProvider;

    public MarkdownGenerator(ResourceTypeProvider typeProvider)
    {
        this.typeProvider = typeProvider;
    }

    public record GroupedTypes(
        string ProviderNamespace,
        ImmutableDictionary<string, ImmutableArray<string>> ResourceTypeByApiVersion)
    {
        public IReadOnlyDictionary<string, string> GetLatestApiVersionByType()
            => ResourceTypeByApiVersion
                .SelectMany(x => x.Value.Select(y => (type: y, version: x.Key)))
                .GroupBy(x => x.type, x => x.version, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y, ApiVersionComparer.Instance).First(), StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, ImmutableArray<string>> GetApiVersionsByType()
            => ResourceTypeByApiVersion
                .SelectMany(x => x.Value.Select(y => (type: y, version: x.Key)))
                .GroupBy(x => x.type, x => x.version, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.ToImmutableArray(), StringComparer.OrdinalIgnoreCase);
    }

    public record GenerateResult(
        GroupedTypes GroupedTypes,
        ImmutableDictionary<string, string> FilesByPath);

    public record PageMetadata(
        DateTime Date,
        string Author,
        string MsAuthor);

    public record ResourceMetadata(
        string Provider,
        string ResourceType,
        string UnqualifiedResourceType,
        string ApiVersion,
        ResourceType Type);

    public record NamedType(string Name, TypeBase Type);

    public IEnumerable<GroupedTypes> GetGroupedTypes()
    {
        var resourceTypesByProvider = typeProvider.GetResourceTypes()
            .GroupBy(x => Utils.GetProviderNamespace(x.resourceType), StringComparer.OrdinalIgnoreCase);

        foreach (var providerGroup in resourceTypesByProvider)
        {
            var providerNamespace = providerGroup.Key;
            var typesByApiVersion = providerGroup
                .GroupBy(x => x.apiVersion, StringComparer.OrdinalIgnoreCase)
                .ToImmutableDictionary(x => x.Key, x => x.Select(y => y.resourceType).ToImmutableArray(), StringComparer.OrdinalIgnoreCase);

            yield return new(providerNamespace, typesByApiVersion);
        }
    }

    public static IEnumerable<KeyValuePair<string, ObjectTypeProperty>> GetOrderedWritableProperties(IEnumerable<KeyValuePair<string, ObjectTypeProperty>> properties)
        => properties.Where(x => !x.Value.Flags.HasFlag(ObjectTypePropertyFlags.ReadOnly))
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

    private static string GetHeading(PageMetadata metadata, ResourceMetadata resource)
    {
        return $"""
---
title: {resource.ResourceType} {resource.ApiVersion}
description: Azure {resource.ResourceType} syntax and properties to use in Azure Resource Manager templates for deploying the resource. API version {resource.ApiVersion}
author: {metadata.Author}
zone_pivot_groups: deployment-languages-reference
ms.service: azure-resource-manager
ms.topic: reference
ms.date: {metadata.Date.ToString("MM/dd/yyyy")}
ms.author: {metadata.MsAuthor}
---
""";
    }

    private static string GetApiVersionLinks(GroupedTypes groupedTypes, ResourceMetadata resource, bool isLatestVersionPage)
    {
        var pathDepth = resource.UnqualifiedResourceType.Split('/').Length;
        if (isLatestVersionPage)
        {
            // The latest pages are one level higher than the other pages
            pathDepth--;
        }

        var pathPrefix = string.Concat(Enumerable.Repeat("../", pathDepth));

        var otherApiVersions = groupedTypes.ResourceTypeByApiVersion
            .Where(x => x.Value.Contains(resource.ResourceType, StringComparer.OrdinalIgnoreCase))
            .Where(x => !StringComparer.OrdinalIgnoreCase.Equals(resource.ApiVersion))
            .Select(x => x.Key)
            .OrderByDescending(x => x, ApiVersionComparer.Instance);
        
        var resourceTypePageName = resource.UnqualifiedResourceType.ToLowerInvariant();

        var sb = new StringBuilder();
        sb.Append($"""
> [!div class="op_single_selector" title1="API Versions:"]
> - [Latest]({pathPrefix}{resourceTypePageName}.md)

""");

        foreach (var otherApiVersion in otherApiVersions)
        {
            var apiVersionLower = otherApiVersion.ToLowerInvariant();
        sb.Append($"""
> - [{apiVersionLower}]({pathPrefix}{apiVersionLower}/{resourceTypePageName}.md)

""");
        }

        return sb.ToString();
    }

    private static string GetResourceRemarks(ResourceMetadata resource, RemarksFile remarks)
    {
        var resourceRemarks = remarks.GetResourceRemarks(resource).ToArray();

        if (!resourceRemarks.Any())
        {
            return "";
        }

        var sb = new StringBuilder();
        sb.Append($"""
## Remarks


""");

        foreach (var resourceRemark in resourceRemarks)
        {
            sb.Append($"""
{resourceRemark.Description}


""");
        }

        return sb.ToString();
    }

    private static string GetBicepZone(ConfigLoader configLoader, RemarksLoader remarksLoader, ResourceMetadata resource, ImmutableArray<NamedType> namedTypes, RemarksFile remarks, int anchorIndex)
    {
        var sb = new StringBuilder();
        sb.Append($"""
::: zone pivot="deployment-language-bicep"

## Bicep resource definition

The {resource.UnqualifiedResourceType} resource type can be deployed with operations that target: 


""");

        if (resource.Type.ScopeType.HasFlag(ScopeType.Tenant))
        {
                    sb.Append($"""
* **Tenant** - See [tenant deployment commands](/azure/azure-resource-manager/bicep/deploy-to-tenant)
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.ManagementGroup))
        {
                    sb.Append($"""
* **Management groups** - See [management group deployment commands](/azure/azure-resource-manager/bicep/deploy-to-management-group)
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.Subscription))
        {
                    sb.Append($"""
* **Subscription** - See [subscription deployment commands](/azure/azure-resource-manager/bicep/deploy-to-subscription)
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.ResourceGroup))
        {
                    sb.Append($"""
* **Resource groups** - See [resource group deployment commands](/azure/azure-resource-manager/bicep/deploy-to-resource-group)
""");
        }

        var samples = CodeSampleGenerator.GetExample(resource, namedTypes, CodeSampleGenerator.Flavor.Bicep);

        sb.Append($"""


For a list of changed properties in each API version, see [change log](~/{resource.Provider.ToLowerInvariant()}/change-log/{resource.UnqualifiedResourceType.ToLowerInvariant()}.md).

## Resource format

To create a {resource.ResourceType} resource, add the following Bicep to your template.

```bicep
{samples.MainSample}
```

""");

        sb.Append(GetDeploymentRemarks(resource, remarks, DeploymentType.Bicep));
        sb.Append(GetBicepSamples(remarksLoader, resource, remarks));

        foreach (var (discObjectType, discSamples) in samples.DiscrimatedSamples)
        {
            sb.Append($"""
### {discObjectType.Name} objects

Set the **{discObjectType.Discriminator}** property to specify the type of object.


""");

            foreach (var discSample in discSamples)
            {
                sb.Append($"""
For **{discSample.DiscriminatorValue}**, use:

```bicep
{discSample.Sample}
```


""");
            }
        }

        sb.Append($"""
## Property values

{GetPropertyValues(resource, namedTypes, remarks, anchorIndex)}

""");

        var quickstartLinks = configLoader.GetSamples().QuickstartLinks;
        if (quickstartLinks.TryGetValue(resource.ResourceType.ToLowerInvariant(), out var links))
        {

            sb.Append($"""
## Quickstart templates

The following quickstart templates deploy this resource type.

> [!div class="mx-tableFixed"]
> | Template | Description |
> | ----- | ----- |

""");
            foreach (var link in links)
            {
                sb.Append($"""
> | [{link.Title}]({link.TemplateUrl})<br><br>[![Deploy to Azure](~/media/deploy-to-azure.svg)]({link.DeployUrl}) | {link.Description} |

""");
            }
        }

        sb.Append($"""

::: zone-end

""");

        return sb.ToString();
    }


    private static string GetArmTemplateZone(ConfigLoader configLoader, ResourceMetadata resource, ImmutableArray<NamedType> namedTypes, RemarksFile remarks, int anchorIndex)
    {
        var sb = new StringBuilder();
        sb.Append($"""
::: zone pivot="deployment-language-arm-template"

## ARM template resource definition

The {resource.UnqualifiedResourceType} resource type can be deployed with operations that target: 


""");

        if (resource.Type.ScopeType.HasFlag(ScopeType.Tenant))
        {
                    sb.Append($"""
* **Tenant** - See [tenant deployment commands](/azure/azure-resource-manager/templates/deploy-to-tenant)
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.ManagementGroup))
        {
                    sb.Append($"""
* **Management groups** - See [management group deployment commands](/azure/azure-resource-manager/templates/deploy-to-management-group)
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.Subscription))
        {
                    sb.Append($"""
* **Subscription** - See [subscription deployment commands](/azure/azure-resource-manager/templates/deploy-to-subscription)
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.ResourceGroup))
        {
                    sb.Append($"""
* **Resource groups** - See [resource group deployment commands](/azure/azure-resource-manager/templates/deploy-to-resource-group)
""");
        }

        var samples = CodeSampleGenerator.GetExample(resource, namedTypes, CodeSampleGenerator.Flavor.Json);

        sb.Append($"""


For a list of changed properties in each API version, see [change log](~/{resource.Provider.ToLowerInvariant()}/change-log/{resource.UnqualifiedResourceType.ToLowerInvariant()}.md).

## Resource format

To create a {resource.ResourceType} resource, add the following JSON to your template.

```json
{samples.MainSample}
```

""");

        sb.Append(GetDeploymentRemarks(resource, remarks, DeploymentType.Json));

        foreach (var (discObjectType, discSamples) in samples.DiscrimatedSamples)
        {
            sb.Append($"""
### {discObjectType.Name} objects

Set the **{discObjectType.Discriminator}** property to specify the type of object.


""");

            foreach (var discSample in discSamples)
            {
                sb.Append($"""
For **{discSample.DiscriminatorValue}**, use:

```json
{discSample.Sample}
```


""");
            }
        }

        sb.Append($"""
## Property values

{GetPropertyValues(resource, namedTypes, remarks, anchorIndex)}

""");

        var quickstartLinks = configLoader.GetSamples().QuickstartLinks;
        if (quickstartLinks.TryGetValue(resource.ResourceType.ToLowerInvariant(), out var links))
        {

            sb.Append($"""
## Quickstart templates

The following quickstart templates deploy this resource type.

> [!div class="mx-tableFixed"]
> | Template | Description |
> | ----- | ----- |

""");
            foreach (var link in links)
            {
                sb.Append($"""
> | [{link.Title}]({link.TemplateUrl})<br><br>[![Deploy to Azure](~/media/deploy-to-azure.svg)]({link.DeployUrl}) | {link.Description} |

""");
            }
        }

        sb.Append($"""

::: zone-end

""");

        return sb.ToString();
    }

    private static string GetTerraformZone(ResourceMetadata resource, ImmutableArray<NamedType> namedTypes, RemarksFile remarks, int anchorIndex)
    {
        var sb = new StringBuilder();
        sb.Append($"""
::: zone pivot="deployment-language-terraform"

## Terraform (AzAPI provider) resource definition

The {resource.UnqualifiedResourceType} resource type can be deployed with operations that target: 


""");

        if (resource.Type.ScopeType.HasFlag(ScopeType.Tenant))
        {
                    sb.Append($"""
* **Tenant**
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.ManagementGroup))
        {
                    sb.Append($"""
* **Management groups**
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.Subscription))
        {
                    sb.Append($"""
* **Subscription**
""");
        }

        if (resource.Type.ScopeType.HasFlag(ScopeType.ResourceGroup))
        {
                    sb.Append($"""
* **Resource groups**
""");
        }

        var samples = CodeSampleGenerator.GetExample(resource, namedTypes, CodeSampleGenerator.Flavor.Terraform);

        sb.Append($"""


For a list of changed properties in each API version, see [change log](~/{resource.Provider.ToLowerInvariant()}/change-log/{resource.UnqualifiedResourceType.ToLowerInvariant()}.md).

## Resource format

To create a {resource.ResourceType} resource, add the following Terraform to your template.

```terraform
{samples.MainSample}
```

""");

        sb.Append(GetDeploymentRemarks(resource, remarks, DeploymentType.Terraform));

        foreach (var (discObjectType, discSamples) in samples.DiscrimatedSamples)
        {
            sb.Append($"""
### {discObjectType.Name} objects

Set the **{discObjectType.Discriminator}** property to specify the type of object.


""");

            foreach (var discSample in discSamples)
            {
                sb.Append($"""
For **{discSample.DiscriminatorValue}**, use:

```terraform
{discSample.Sample}
```


""");
            }
        }

        sb.Append($"""
## Property values

{GetPropertyValues(resource, namedTypes, remarks, anchorIndex)}
::: zone-end

""");

        return sb.ToString();
    }

    private static string GetBicepSamples(RemarksLoader remarksLoader, ResourceMetadata resource, RemarksFile remarks)
    {
        var bicepSamples = remarks.GetBicepSamples(resource).ToArray();

        if (!bicepSamples.Any())
        {
            return "";
        }

        var sb = new StringBuilder();
        sb.Append($"""
## Examples


""");
        
        foreach (var sample in bicepSamples)
        {
            sb.Append($"""
{sample.Description}

```bicep
{remarksLoader.GetCodeSample(resource.Provider, sample)}
```

""");
        }

        return sb.ToString();
    }

    private static string GetDeploymentRemarks(ResourceMetadata resource, RemarksFile remarks, DeploymentType deploymentType)
    {
        var sb = new StringBuilder();
        
        foreach (var deploymentRemark in remarks.GetDeploymentRemarks(resource, deploymentType))
        {
            sb.Append($"""
> [!NOTE]
> {deploymentRemark.Description}


""");
        }

        return sb.ToString();
    }

    private static ImmutableArray<NamedType> GetNamedTypes(ResourceMetadata resource)
    {
        HashSet<TypeBase> visited = new();
        Queue<TypeBase> queue = new();
        List<NamedType> namedTypes = new();

        queue.Enqueue(resource.Type);

        while (queue.Any())
        {
            var type = queue.Dequeue();
            if (visited.Contains(type))
            {
                continue;
            }
            visited.Add(type);

            switch (type)
            {
                case ResourceType resourceType:
                    queue.Enqueue(resourceType.Body.Type);
                    break;

                case ObjectType objectType:
                    namedTypes.Add(new(objectType.Name, type));
                    foreach (var (_, prop) in GetOrderedWritableProperties(objectType.Properties))
                    {
                        queue.Enqueue(prop.Type.Type);
                    }
                    if (objectType.AdditionalProperties != null)
                    {
                        queue.Enqueue(objectType.AdditionalProperties.Type);
                    }
                    break;
                case DiscriminatedObjectType discObjectType:
                    namedTypes.Add(new(discObjectType.Name, type));
                    foreach (var (_, prop) in GetOrderedWritableProperties(discObjectType.BaseProperties))
                    {
                        queue.Enqueue(prop.Type.Type);
                    }
                    foreach (var element in discObjectType.Elements.Values)
                    {
                        queue.Enqueue(element.Type);
                    }
                    break;
                case ArrayType arrayType:
                    if (arrayType.ItemType != null)
                    {
                        queue.Enqueue(arrayType.ItemType.Type);
                    }
                    break;
                case UnionType unionType:
                    foreach (var element in unionType.Elements)
                    {
                        queue.Enqueue(element.Type);
                    }
                    break;                
            }
        }

        return [.. namedTypes];
    }

    private static string Br => "<br />";

    private static string JoinWithBr(IEnumerable<string> values)
        => string.Join(Br, values);

    private static string JoinWithBr(params string[] values)
        => JoinWithBr(values as IEnumerable<string>);

    private static string GetTypeValue(TypeBase type, int anchorIndex)
    {
        switch (type)
        {
            case UnionType unionType when unionType.Elements.Select(x => x.Type).OfType<StringLiteralType>() is {} elements:
                return JoinWithBr(elements.Select(GetTypeValue).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
            case StringLiteralType stringLiteral:
                return $"'{stringLiteral.Value}'";
            case IntegerType intType:
                var intConstraints = GetConstraintText(intType);
                return $"int{intConstraints}";
            case BooleanType:
                return "bool";
            case AnyType anyType:
                return "any";
            case StringType stringType:
                var stringConstraints = GetConstraintText(stringType);
                return $"string{stringConstraints}";
            case ObjectType objectType:
                var anchorSuffixO = anchorIndex > 0 ? $"-{anchorIndex}" : "";
                return MarkdownUtils.GetLink(objectType.Name, $"#{objectType.Name.ToLowerInvariant()}{anchorSuffixO}");
            case DiscriminatedObjectType objectType:
                var anchorSuffixDo = anchorIndex > 0 ? $"-{anchorIndex}" : "";
                return MarkdownUtils.GetLink(objectType.Name, $"#{objectType.Name.ToLowerInvariant()}{anchorSuffixDo}");
            case ArrayType arrayType when arrayType.ItemType.Type is UnionType:
                return $"String array containing any of:{Br}{GetTypeValue(arrayType.ItemType.Type, anchorIndex)}";
            case ArrayType arrayType:
                return $"{GetTypeValue(arrayType.ItemType.Type, anchorIndex)}[]";
            default:
                throw new NotImplementedException($"Type {type.GetType()} not implemented");
        }
    }

    private static string GetConstraintText(StringType stringType)
    {
        var constraints = new List<string>();
        
        if (stringType.MinLength is not null)
        {
            constraints.Add($"Min length = {stringType.MinLength}");
        }
        if (stringType.MaxLength is not null)
        {
            constraints.Add($"Max length = {stringType.MinLength}");
        }
        if (stringType.Sensitive is true)
        {
            constraints.Add($"Sensitive value. Pass in as a secure parameter.");
        }
        if (stringType.Pattern is not null)
        {
            constraints.Add($"Pattern = `{stringType.Pattern}`");
        }

        return constraints.Any() ? $" {Br}{Br}" + JoinWithBr("Constraints:", JoinWithBr(constraints)) : "";
    }

    private static string GetConstraintText(IntegerType intType)
    {
        var constraints = new List<string>();
        
        if (intType.MinValue is not null)
        {
            constraints.Add($"Min value = {intType.MinValue}");
        }
        if (intType.MaxValue is not null)
        {
            constraints.Add($"Max value = {intType.MaxValue}");
        }

        return constraints.Any() ? $" {Br}{Br}" + JoinWithBr("Constraints:", JoinWithBr(constraints)) : "";
    }

    public static string GetPropertyValues(ResourceMetadata resource, ImmutableArray<NamedType> namedTypes, RemarksFile remarks, int anchorIndex)
    {
        var anchorSuffix = anchorIndex > 0 ? $"-{anchorIndex}" : "";
        var remarksByObjectName = remarks.GetPropertyRemarks(resource).ToLookup(x => x.ObjectName, StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        void writeHeading(string name) {
                    sb.Append($"""
### {name}

| Name | Description | Value |
| ---- | ----------- | ------------ |

""");
        }

        void writeProperties(string name, IReadOnlyDictionary<string, ObjectTypeProperty> properties) {
            var remarksByPropertyName = remarksByObjectName[name]
                .ToDictionary(x => x.PropertyName, StringComparer.OrdinalIgnoreCase);

            foreach (var (propName, prop) in GetOrderedWritableProperties(properties))
            {
                var remark = remarksByPropertyName.TryGetValue(propName, out var value) ? value : null;

                var requiredSuffix = prop.Flags.HasFlag(ObjectTypePropertyFlags.Required) ? " (required)" : "";

                sb.Append($"""
| {propName} | {remark?.Description ?? prop.Description} | {GetTypeValue(prop.Type.Type, anchorIndex)}{requiredSuffix} |

""");
            }
        }

        foreach (var (name, type) in namedTypes.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            switch (type)
            {
                case ObjectType objectType: {
                    writeHeading(name);
                    writeProperties(name, objectType.Properties);
                    sb.AppendLine("");
                    break;
                }
                case DiscriminatedObjectType discType: {
                    var typeLookup = discType.Elements
                        .ToDictionary(x => x.Key, x => (x.Value.Type as ObjectType)!);


                    if (typeLookup.Values.Any(x => x is null))
                    {
                        throw new InvalidOperationException($"Expected all discriminated elements to be objects");
                    }

                    var discriminatorProperty = new ObjectTypeProperty(
                        new StaticTypeReference(new UnionType(discType.Elements.Keys
                            .Select(x => new StaticTypeReference(new StringLiteralType(x)))
                            .ToList())),
                        ObjectTypePropertyFlags.Required,
                        string.Join(' ', typeLookup.Select(x => $"Set to '{x.Key}' for type {MarkdownUtils.GetLink(x.Value.Name, $"#{x.Value.Name.ToLowerInvariant()}{anchorSuffix}")}.")));

                    writeHeading(name);
                    writeProperties(name, discType.BaseProperties.ToImmutableDictionary().Add(discType.Discriminator, discriminatorProperty));
                    sb.AppendLine("");
                    break;
                }
            }
            
        }

        return sb.ToString();
    }

    public static string GenerateMarkdown(PageMetadata pageMetadata, GroupedTypes groupedTypes, ResourceType resourceType, ConfigLoader configLoader, RemarksLoader remarksLoader, bool isLatestVersionPage)
    {
        var resourceTypeName = resourceType.Name.Split('@')[0];
        var apiVersion = resourceType.Name.Split('@')[1];
        var providerNamespace = Utils.GetProviderNamespace(resourceTypeName);
        var unqualifiedResourceType = Utils.GetUnqualifiedType(resourceTypeName);

        var resource = new ResourceMetadata(
            Provider: providerNamespace,
            ResourceType: resourceTypeName,
            UnqualifiedResourceType: unqualifiedResourceType,
            ApiVersion: apiVersion,
            Type: resourceType);

        var namedTypes = GetNamedTypes(resource);
        var remarks = remarksLoader.GetRemarks(providerNamespace);

        return $"""
{GetHeading(pageMetadata, resource)}
# {resource.Provider} {resource.UnqualifiedResourceType} {resource.ApiVersion}

{GetApiVersionLinks(groupedTypes, resource, isLatestVersionPage)}
{GetResourceRemarks(resource, remarks)}
{GetBicepZone(configLoader, remarksLoader, resource, namedTypes, remarks, 0)}
{GetArmTemplateZone(configLoader, resource, namedTypes, remarks, 1)}
{GetTerraformZone(resource, namedTypes, remarks, 2)}
""";
    }
}
Param (
    [parameter(Mandatory=$true)][string] $QuickStartsRepoPath
)

$ErrorActionPreference = "Stop"

$QuickStartsRepoPath = Resolve-Path $QuickStartsRepoPath
$OutputPath = Join-Path $PSScriptRoot "../settings/samples/samples.json"

function GetQuickstartLinks {
    function GetResourceTypes {
        param([PSCustomObject] $template)

        $resourceTypes = @()
        foreach ($resource in $template.resources) {
            if ($resource.type -ieq "Microsoft.Resources/deployments") {
                foreach ($resourceType in GetResourceTypes($resource.properties.template)) {
                    $resourceTypes += $resourceType
                }
            } else {
                $resourceTypes += $resource.type
            }
        }

        return $resourceTypes | Sort-Object -Unique
    }

    $links = @()
    $metadataPaths = Get-ChildItem $QuickStartsRepoPath -File -Recurse -Include "metadata.json" | Sort-Object FullName
    foreach ($metadataPath in $metadataPaths) {
        Write-Host "Processing $metadataPath"
        $templatePath = Join-Path $metadataPath.DirectoryName "azuredeploy.json" -Resolve
        $bicepPath = Join-Path $metadataPath.DirectoryName "main.bicep"
        $hasBicep = (Test-Path $bicepPath -PathType Leaf)

        $metadataContent = Get-Content $metadataPath.FullName | ConvertFrom-Json
        $description = $metadataContent.description
        $displayName = $metadataContent.itemDisplayName

        $templateContent = Get-Content $templatePath | ConvertFrom-Json
        $resourceTypes = GetResourceTypes($templateContent)

        $links += [ordered]@{
            Title = $displayName;
            Description = $description;
            Path = [System.IO.Path]::GetRelativePath($QuickStartsRepoPath, "$templatePath/..").Replace("\", "/")
            ResourceTypes = @($resourceTypes);
            HasBicep = $hasBicep;
        }
    }

    return $links
}

function GetAvmLinks {
    $bicepLinks = "https://raw.githubusercontent.com/Azure/Azure-Verified-Modules/refs/heads/main/docs/static/module-indexes/BicepResourceModules.csv"
    $tfLinks = "https://raw.githubusercontent.com/Azure/Azure-Verified-Modules/refs/heads/main/docs/static/module-indexes/TerraformResourceModules.csv"

    $bicepModules = ConvertFrom-Csv (Invoke-WebRequest -Uri $bicepLinks).Content
    $tfModules = ConvertFrom-Csv (Invoke-WebRequest -Uri $tfLinks).Content

    $availableStatuses = @(
        "Available :green_circle:",
        "Orphaned :eyes:"
    )

    $links = @()
    foreach ($bicepModule in $bicepModules) {
        Write-Host "Processing $($bicepModule.RepoURL)"

        if ($bicepModule.ModuleStatus -notin $availableStatuses) {
            continue
        }

        $links += [ordered]@{
            Type = "Bicep";
            Title = $bicepModule.ModuleDisplayName;
            Description = $bicepModule.Description;
            ResourceType = "$($bicepModule.ProviderNamespace)/$($bicepModule.ResourceType)";
            RepoUrl = $bicepModule.RepoURL;
        }
    }
    foreach ($tfModule in $tfModules) {
        Write-Host "Processing $($tfModule.RepoURL)"

        if ($tfModule.ModuleStatus -notin $availableStatuses) {
            continue
        }

        $links += [ordered]@{
            Type = "Terraform";
            Title = $tfModule.ModuleDisplayName;
            Description = $tfModule.Description;
            ResourceType = "$($tfModule.ProviderNamespace)/$($tfModule.ResourceType)";
            RepoUrl = $tfModule.RepoURL;
        }
    }

    return $links
}

$quickstartLinks = GetQuickstartLinks
$avmLinks = GetAvmLinks

$samples = [ordered]@{
    "`$schema" = "../schemas/samples.schema.json";
    QuickstartLinks = $quickstartLinks;
    AvmLinks = $avmLinks;
}

$samples | ConvertTo-Json -depth 100 | Out-File $OutputPath
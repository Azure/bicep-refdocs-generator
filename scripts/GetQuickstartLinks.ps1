Param (
    [parameter(Mandatory=$true)][string] $QuickStartsRepoPath
)

$ErrorActionPreference = "Stop"

$QuickStartsRepoPath = Resolve-Path $QuickStartsRepoPath
$OutputPath = Join-Path $PSScriptRoot "../src/TemplateRefGenerator/Files/samples/samples.json"

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

$quickstartLinks = @()
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

    $quickstartLinks += [ordered]@{
        Title = $displayName;
        Description = $description;
        Path = [System.IO.Path]::GetRelativePath($QuickStartsRepoPath, "$templatePath/..").Replace("\\", "/")
        ResourceTypes = @($resourceTypes);
        HasBicep = $hasBicep;
    }
}

$samples = [ordered]@{
    "`$schema" = "../samples.schema.json";
    QuickstartLinks = $quickstartLinks;
}

$samples | ConvertTo-Json -depth 100 | Out-File $OutputPath
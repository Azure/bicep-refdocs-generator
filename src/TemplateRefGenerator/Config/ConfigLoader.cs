// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Immutable;

namespace TemplateRefGenerator;

public class ConfigLoader
{
    private Lazy<ConfigFile> ConfigFileLazy = 
        new(() => Utils.DeserializeJsonFile<ConfigFile>("config/config.json", Utils.ReadManifestFile("config/config.json")));

    private Lazy<SamplesFile> SampleFileLazy = 
        new(() => Utils.DeserializeJsonFile<SamplesFile>("samples/samples.json", Utils.ReadManifestFile("samples/samples.json")));

    public record ConfigFile(
        ImmutableDictionary<string, string> TocTitleMapping,
        ImmutableArray<string> ExcludedProviders);

    public record SamplesFile(
        ImmutableDictionary<string, ImmutableArray<QuickstartLink>> QuickstartLinks);

    public record QuickstartLink(
        string Title,
        string Description,
        string TemplateUrl,
        string DeployUrl);

    public ConfigFile GetConfiguration()
        => ConfigFileLazy.Value;

    public SamplesFile GetSamples()
        => SampleFileLazy.Value;
}
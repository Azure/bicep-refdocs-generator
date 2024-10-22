// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TemplateRefGenerator;

public static class Utils
{
    public static string GetProviderNamespace(string resourceType)
        => resourceType.Split('/', 2)[0];

    public static string GetUnqualifiedType(string resourceType)
        => resourceType.Split('/', 2)[1];

    public static string? TryReadManifestFile(params string[] pathComponents)
        => typeof(Utils).Assembly.GetManifestResourceStream(GetManifestFilePath(pathComponents)) is Stream stream
            ? new StreamReader(stream).ReadToEnd()
            : null;

    public static string ReadManifestFile(params string[] pathComponents)
        => TryReadManifestFile(pathComponents) ?? throw new InvalidOperationException($"File {GetManifestFilePath(pathComponents)} does not exist");

    public static string GetManifestFilePath(string[] pathComponents)
        => $"Files/{string.Join('/', pathComponents)}";

    public static T DeserializeJsonFile<T>(string filePath, string contents)
    {
        var options = new JsonSerializerOptions
        {
            Converters = 
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        return JsonSerializer.Deserialize<T>(contents, options) ??
            throw new InvalidOperationException($"Failed to deserialize file {filePath}");
    }

    public static bool IsChildResource(string unqualifiedResourceType)
        => unqualifiedResourceType.Contains('/');

    public static string GetParentResource(string resourceType)
        => resourceType.Substring(0, resourceType.LastIndexOf('/'));
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TestHelpers;
public record EmbeddedFile(
    Assembly Assembly,
    string StreamPath)
{
    private readonly Lazy<string?> contentsLazy = new(() =>
    {
        var stream = Assembly.GetManifestResourceStream(StreamPath);
        if (stream is null)
        {
            return null;
        }

        return new StreamReader(stream).ReadToEnd();
    });

    public string? Contents => contentsLazy.Value;

    public string FileName => Path.GetFileName(StreamPath);

    public string RelativeSourcePath => BaselineHelper.CombinePaths("src", Assembly.GetName().Name!, StreamPath);

    public static IEnumerable<EmbeddedFile> LoadAll(Assembly assembly, string streamPathPrefix, Func<string, bool> shouldLoad)
    {
        // Set the convention that all embedded resource files are in a folder named "Files"
        var combinedPathPrefix = $"Files/{streamPathPrefix}/";

        return LoadAll(assembly, name => name.StartsWith(combinedPathPrefix, StringComparison.Ordinal) && shouldLoad(name));
    }

    public static IEnumerable<EmbeddedFile> LoadAll(Assembly assembly, Regex regex)
        => LoadAll(assembly, regex.IsMatch);

    public static IEnumerable<EmbeddedFile> LoadAll(Assembly assembly, Func<string, bool> shouldLoad)
    {
        foreach (var streamName in assembly.GetManifestResourceNames().Where(shouldLoad))
        {
            if (shouldLoad(streamName))
            {
                yield return new(assembly, streamName);
            }
        }
    }

    public override string ToString() => StreamPath;
}

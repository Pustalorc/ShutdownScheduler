using System;
using System.Collections.Generic;
using CommandLine;
using Pustalorc.Libraries.RocketModCommandsExtended.Abstractions.WithParsing;

namespace Pustalorc.Unturned.Plugins.ShutdownScheduler.Commands.Parsing;

[Serializable]
public sealed class ShutdownParsing : CommandParsing
{
    [Value(0, Required = false)] public uint? Time { get; set; }

    [Value(1, Max = 100, Required = false)]
    public IEnumerable<string>? Reason { get; set; }
}
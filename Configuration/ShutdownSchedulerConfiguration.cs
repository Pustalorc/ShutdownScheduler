using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;

namespace Pustalorc.Unturned.Plugins.ShutdownScheduler.Configuration;

[Serializable]
public sealed class ShutdownSchedulerConfiguration : IRocketPluginConfiguration
{
    public uint ShutdownSchedulingTime { get; set; } = 21600U;

    public ushort ShutdownWarningEffectId { get; set; } = 0;

    public string ShutdownWarningEffectChildElementName { get; set; } = "warning_text";

    public List<string> CommandsToExecuteBeforeShutdown { get; set; } = [];

    [XmlArray("TimeOffset")] public List<uint> ShutdownWarningTimes { get; set; } = [];

    public void LoadDefaults()
    {
        CommandsToExecuteBeforeShutdown.Add("save");
        ShutdownWarningTimes.Add(30U);
        ShutdownWarningTimes.Add(60U);
        ShutdownWarningTimes.Add(120U);
    }
}
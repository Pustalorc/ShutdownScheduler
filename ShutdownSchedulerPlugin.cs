using System.Collections.Generic;
using Pustalorc.Libraries.RocketModCommandsExtended.Abstractions;
using Pustalorc.Libraries.RocketModCommandsExtended.Extensions;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Commands;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Configuration;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Tasks;
using Rocket.API.Collections;
using Rocket.Core.Plugins;

namespace Pustalorc.Unturned.Plugins.ShutdownScheduler;

public sealed class ShutdownSchedulerPlugin : RocketPlugin<ShutdownSchedulerConfiguration>
{
    internal const string ShutdownWarningTranslationKey = "shutdown_warning";
    internal const string FormatterTranslationKey = "warning_message_format";

    private ShutdownTask ShutdownTask { get; }

    private List<RocketCommandWithTranslations> Commands { get; }

    public override TranslationList DefaultTranslations => new()
    {
        { ShutdownWarningTranslationKey, "Warning! The server will shut down in {0}!" },
        { FormatterTranslationKey, "mm' minutes'ss' seconds'" }
    };

    public ShutdownSchedulerPlugin()
    {
        ShutdownTask = new ShutdownTask(Configuration.Instance, Translate);

        var translations = this.GetCurrentTranslationsForCommands();
        Commands = new List<RocketCommandWithTranslations>
        {
            new StartShutdownCommand(ShutdownTask, translations),
            new StopShutdownCommand(ShutdownTask, translations)
        };
    }

    protected override void Load()
    {
        ShutdownTask.ReloadConfiguration(Configuration.Instance);
        Commands.ReloadCommands(this);
        Commands.LoadAndRegisterCommands(this);
    }

    protected override void Unload()
    {
        ShutdownTask.Cancel();
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pustalorc.Libraries.RocketModCommandsExtended.Abstractions;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Tasks;
using Rocket.API;

namespace Pustalorc.Unturned.Plugins.ShutdownScheduler.Commands;

internal sealed class StopShutdownCommand(ShutdownTask task, Dictionary<string, string> translations)
    : RocketCommandWithTranslations(true, translations, StringComparer.OrdinalIgnoreCase)
{
    private const string ShutdownAlreadyCancelledTranslationKey = "shutdown_already_cancelled";

    private const string ShutdownCancelledTranslationKey = "shutdown_cancelled";

    public override AllowedCaller AllowedCaller => AllowedCaller.Both;

    public override string Name => "stopShutdown";

    public override string Help => "Stops the queued shutdown task.";

    public override string Syntax => "";

    public override Dictionary<string, string> DefaultTranslations =>
        new()
        {
            { ShutdownAlreadyCancelledTranslationKey, "The shutdown cannot be stopped as it is not running." },
            { ShutdownCancelledTranslationKey, "The shutdown has been stopped!" }
        };

    private ShutdownTask ShutdownTask { get; } = task;

    public override Task ExecuteAsync(IRocketPlayer caller, string[] command)
    {
        if (ShutdownTask.IsCancelled)
        {
            SendTranslatedMessage(caller, ShutdownAlreadyCancelledTranslationKey, []);
            return Task.CompletedTask;
        }

        ShutdownTask.Cancel();
        SendTranslatedMessage(caller, ShutdownCancelledTranslationKey, []);
        return Task.CompletedTask;
    }
}
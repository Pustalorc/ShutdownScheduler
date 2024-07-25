using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pustalorc.Libraries.RocketModCommandsExtended.Abstractions;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Commands.Parsing;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Tasks;
using Rocket.API;

namespace Pustalorc.Unturned.Plugins.ShutdownScheduler.Commands;

internal sealed class StartShutdownCommand(ShutdownTask task, Dictionary<string, string> translations)
    : RocketCommandWithParsing<ShutdownParsing>(true, translations, StringComparer.OrdinalIgnoreCase)
{
    private const string ShutdownAlreadyStartedTranslationKey = "shutdown_already_started";

    private const string ShutdownStartedTranslationKey = "shutdown_started";

    private const string ShutdownReasonMessage = "shutdown_started_reason";

    public override AllowedCaller AllowedCaller => AllowedCaller.Both;

    public override string Name => "startShutdown";

    public override string Help => "Starts the queued shutdown task.";

    public override string Syntax => "<time> [reason]";

    public override Dictionary<string, string> DefaultTranslations => new()
    {
        { ShutdownAlreadyStartedTranslationKey, "The shutdown already is running and cannot be started again." },
        { ShutdownStartedTranslationKey, "The shutdown has been started." },
        { ShutdownReasonMessage, "A shutdown has been scheduled for: {0}" }
    };

    private ShutdownTask ShutdownTask { get; } = task;

    public override Task ExecuteAsync(IRocketPlayer caller, ShutdownParsing args)
    {
        if (!ShutdownTask.IsCancelled)
        {
            SendTranslatedMessage(caller, ShutdownAlreadyStartedTranslationKey, []);
            return Task.CompletedTask;
        }

        if (args.Reason != null) SendTranslatedMessage(ShutdownReasonMessage, string.Join(" ", args.Reason));
        ShutdownTask.Restart(args.Time);
        SendTranslatedMessage(ShutdownStartedTranslationKey, []);
        return Task.CompletedTask;
    }
}
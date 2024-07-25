using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pustalorc.Libraries.AsyncThreadingUtils.TaskQueue.QueueableTasks;
using Pustalorc.Plugins.AsynchronousTaskDispatcher.Dispatcher;
using Pustalorc.Unturned.Plugins.ShutdownScheduler.Configuration;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using SDG.Unturned;

namespace Pustalorc.Unturned.Plugins.ShutdownScheduler.Tasks;

internal sealed class ShutdownTask : QueueableTask
{
    private Func<string, object[], string> Translate { get; }

    private ushort ShutdownWarningEffectId { get; set; }

    private string ShutdownWarningEffectChildElementName { get; set; }

    private uint Duration { get; set; }

    private List<string> ShutdownCommands { get; set; }

    private List<uint> ShutdownWarningTimes { get; set; }

    private uint LastWarningTime { get; set; }

    public ShutdownTask(ShutdownSchedulerConfiguration config, Func<string, object[], string> translate)
    {
        IsRepeating = true;
        Translate = translate;
        ShutdownWarningEffectId = config.ShutdownWarningEffectId;
        ShutdownWarningEffectChildElementName = config.ShutdownWarningEffectChildElementName;
        Duration = config.ShutdownSchedulingTime;
        ShutdownCommands = config.CommandsToExecuteBeforeShutdown;
        ShutdownWarningTimes = config.ShutdownWarningTimes;
        ShutdownWarningTimes.Sort((a, b) => b.CompareTo(a));
        var num = ShutdownWarningTimes[0];
        Delay = (Duration - num) * 1000U;
        LastWarningTime = num;
        UnixTimeStampToExecute = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Delay;
    }

    public void ReloadConfiguration(ShutdownSchedulerConfiguration config)
    {
        ShutdownWarningEffectId = config.ShutdownWarningEffectId;
        ShutdownWarningEffectChildElementName = config.ShutdownWarningEffectChildElementName;
        ShutdownCommands = config.CommandsToExecuteBeforeShutdown;
        ShutdownWarningTimes = config.ShutdownWarningTimes;
        ShutdownWarningTimes.Sort((a, b) => b.CompareTo(a));
        Restart(config.ShutdownSchedulingTime);
    }

    public void Restart(uint? time)
    {
        Logger.Log("Starting (or restarting) shutdown task.");
        IsCancelled = false;
        IsRepeating = true;
        Logger.Log("Set IsCancelled and IsRepeating to false and true respectively.");
        if (time != null)
        {
            Duration = time.Value;
            Logger.Log($"Set the duration of the task to {time} seconds.");
        }

        var num = ShutdownWarningTimes[ShutdownWarningTimes.FindIndex(k => Duration >= k)];
        Logger.Log($"Selected {num} as the first warning time to run, out of a total of {ShutdownWarningTimes.Count} warning times.");
        Delay = (Duration - num) * 1000U;
        Logger.Log($"Set delay to {Delay}ms. It should be the duration of the task - the selected warning time * 1000");
        LastWarningTime = num;
        UnixTimeStampToExecute = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Delay;
        Logger.Log($"Task should execute at this unix timestamp: {UnixTimeStampToExecute}");
        AsyncTaskDispatcher.QueueTask(this);
        Logger.Log("Self-queued.");
    }

    protected override Task Execute(CancellationToken token)
    {
        Logger.Log("Task execution requested.");
        if (Delay == 1000L)
        {
            var num = ShutdownWarningTimes[0];
            Delay = (Duration - num) * 1000U;
            LastWarningTime = num;
            return Task.CompletedTask;
        }

        var num2 = ShutdownWarningTimes.IndexOf(LastWarningTime);
        Logger.Log($"Checked last warning index: {num2}");
        if (num2 == -1)
        {
            Logger.Log("Warning index is out of bounds! Detected that its time to execute the primary task.");
            TaskDispatcher.QueueOnMainThread(InitiateShutdown);
            Logger.Log("Set IsRepeating and IsCancelled to false and true respectively");
            IsRepeating = false;
            IsCancelled = true;
            return Task.CompletedTask;
        }

        WarningMessage(LastWarningTime);
        num2++;
        var num3 = num2 >= ShutdownWarningTimes.Count ? 0U : ShutdownWarningTimes[num2];
        Delay = (num2 >= ShutdownWarningTimes.Count ? LastWarningTime : LastWarningTime - num3) * 1000U;
        LastWarningTime = num3;
        Logger.Log($"upcoming warning is {LastWarningTime}s.");
        return Task.CompletedTask;
    }

    private void WarningMessage(uint timeRemaining)
    {
        Logger.Log($"Sending warning message with {timeRemaining} seconds left.");
        var timeRemainingFormatted = TimeSpan.FromSeconds(timeRemaining)
            .ToString(Translate("warning_message_format", []));
        TaskDispatcher.QueueOnMainThread(delegate
        {
            if (ShutdownWarningEffectId == 0)
            {
                UnturnedChat.Say(Translate("shutdown_warning", [timeRemainingFormatted]));
                return;
            }

            foreach (var steamPlayer in Provider.clients.Where(client => client != null))
                EffectManager.sendUIEffectText((short)ShutdownWarningEffectId, steamPlayer.transportConnection, true,
                    ShutdownWarningEffectChildElementName, timeRemainingFormatted);
        });
    }

    private void InitiateShutdown()
    {
        var consolePlayer = new ConsolePlayer();
        foreach (var text in ShutdownCommands) R.Commands.Execute(consolePlayer, text);
        SaveManager.save();
        Provider.shutdown();
    }
}
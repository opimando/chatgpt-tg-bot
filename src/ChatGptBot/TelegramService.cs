#region Copyright

/*
 * File: TelegramService.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TgBotFramework.Core;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace ChatGptBot;

public class TelegramService : IHostedService
{
    private readonly TelegramBot _bot;
    private readonly IEventBus _eventsBus;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(TelegramBot bot, IEventBus eventsBus, ILogger<TelegramService> logger)
    {
        _bot = bot;
        _eventsBus = eventsBus;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _eventsBus.Subscribe<BaseEvent>(@event =>
        {
            if (@event is IStructuredEvent structuredEvent)
                _logger.Log(GetLevel(structuredEvent.Level), structuredEvent.Template, structuredEvent.Items);
            else
                _logger.LogDebug(@event.ToString());
        });

        _bot.Start();
        await _bot.SetDescription("Бот-провайдер к ChatGpt");
        await _bot.SetCommands(new List<CommandButton>
        {
            new("/refresh", "Обновить контекст"),
            new("/help", "Помощь"),
            new("/settings", "Настройки")
        });
        _logger.LogInformation("application started");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _bot.Stop();
        _logger.LogInformation("application stopped");
        return Task.CompletedTask;
    }

    private LogLevel GetLevel(TgBotFramework.Core.LogLevel level)
    {
        return level switch
        {
            TgBotFramework.Core.LogLevel.Trace => LogLevel.Trace,
            TgBotFramework.Core.LogLevel.Debug => LogLevel.Debug,
            TgBotFramework.Core.LogLevel.Information => LogLevel.Information,
            TgBotFramework.Core.LogLevel.Warning => LogLevel.Warning,
            TgBotFramework.Core.LogLevel.Error => LogLevel.Error,
            TgBotFramework.Core.LogLevel.Critical => LogLevel.Critical,
            _ => LogLevel.None
        };
    }
}
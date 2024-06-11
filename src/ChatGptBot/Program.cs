#region Copyright

/*
 * File: Program.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using System.Reflection;
using Bot.DbModel;
using ChatGptBot.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using Serilog;
using TgBotFramework.Core;
using TgBotFramework.Persistent;

namespace ChatGptBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        IHostBuilder hostBuilder = new HostBuilder()
            .ConfigureHostConfiguration(config =>
            {
                config.AddJsonFile("settings.json", true);
                config.AddJsonFile("logger.json", true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                DbSettings dbSettings = hostContext.Configuration.Get<DbSettings>() ?? new DbSettings();
                var telegramSettings = hostContext.Configuration.Get<TelegramSettings>();
                var openAiSettings = hostContext.Configuration.Get<OpenAiSettings>();
                if (telegramSettings == null)
                    throw new Exception("Не удалось получить настройки Telegram");
                if (openAiSettings == null)
                    throw new Exception("Не удалось получить настройки OpenAi");

                services.AddSingleton<IHostedService, TelegramService>();

                services.AddSingleton(new ChatHolder());
                services.AddDbContextFactory<BotDbContext>((_, builder) =>
                {
                    builder.UseNpgsql(dbSettings.ConnectionString);
                });
                services.AddTransient<IBotRepository, BotRepository>();

                services.AddTransient<IAdminAccessResolver, AdminAccessResolver>();
                services.AddTransient<PartAnswerSender>();
                services.AddSingleton<IOpenAIAPI>(new OpenAIAPI(new APIAuthentication(openAiSettings.OpenAiToken)));

                services.InitializeBot(telegramSettings.ApiKey, builder =>
                    {
                        builder.AuthProviderRegistrationFunction =
                            (sc, _) => sc.AddTransient<IAuthProvider, UserAuthAccessProvider>();
                        builder.WithPersistentStore();
                        builder.WithSpamFilter(15);
                        builder.WithStates(Assembly.GetExecutingAssembly());
                    })
                    .WithPersistent(dbSettings.ConnectionString);
            })
            .UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration))
            .ConfigureLogging((host, config) =>
            {
                if (!host.Configuration.GetChildren().Any(s => s.Key.StartsWith("Serilog")))
                    config.AddConsole();
            });

        IHost build = hostBuilder.Build();
        await build.Services.MigrateBotStore();
        await build.Services.MigrateStateStore();
        await build.RunAsync();
    }
}

public static class StoreExtensions
{
    public static async Task MigrateBotStore(this IServiceProvider serviceProvider)
    {
        await using BotDbContext context = await serviceProvider
            .GetRequiredService<IDbContextFactory<BotDbContext>>()
            .CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }
}
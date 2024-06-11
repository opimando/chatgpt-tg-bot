#region Copyright

/*
 * File: ChatCommand.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel;
using Bot.DbModel.Models;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using TgBotFramework.Core;
using User = Bot.DbModel.Models.User;

namespace ChatGptBot.Commands;

[TelegramState("/chat", "/refresh")]
public class ChatCommand : BaseChatState
{
    private readonly IOpenAIAPI _client;
    private readonly ChatHolder _holder;
    private readonly IBotRepository _botRepository;
    private readonly ILogger<PartAnswerSender> _partSenderLogger;
    private readonly ILogger<ChatCommand> _logger;

    public ChatCommand(
        IOpenAIAPI client,
        ChatHolder holder,
        IBotRepository botRepository,
        IEventBus bus,
        ILogger<PartAnswerSender> partSenderLogger,
        ILogger<ChatCommand> logger
    ) : base(bus)
    {
        _client = client;
        _holder = holder;
        _botRepository = botRepository;
        _partSenderLogger = partSenderLogger;
        _logger = logger;
    }

    private Conversation GetOrCreateConversation(string chatId)
    {
        Conversation? conversation = _holder.Get(chatId);
        if (conversation == null)
        {
            conversation = _client.Chat.CreateConversation();
            conversation.Model = Model.ChatGPTTurbo;
            _holder.AddOrUpdate(chatId, conversation);
        }

        return conversation;
    }

    private Conversation RefreshConversation(string chatId)
    {
        Conversation? conversation = _client.Chat.CreateConversation();
        conversation.Model = Model.ChatGPTTurbo;
        _holder.AddOrUpdate(chatId, conversation);
        return conversation;
    }

    private async Task RefreshContextIfTooLarge(ChatId chatId, Conversation conversation, string requestText,
        IMessenger messenger)
    {
        if (conversation.Messages.Sum(m => m.TextContent.Length) + requestText.Length >= 4000)
        {
            RefreshConversation(chatId.ToString());
            await messenger.Send(chatId, "Обновили контекст");
        }
    }

    protected override async Task<IChatState?> InternalProcessMessage(Message receivedMessage, IMessenger messenger)
    {
        if (receivedMessage.Content is CallbackInlineButtonContent callback &&
            !string.IsNullOrWhiteSpace(callback.Data)
            && callback.Data.Equals("/refresh", StringComparison.OrdinalIgnoreCase))
        {
            RefreshConversation(receivedMessage.ChatId.ToString());
            await messenger.Send(receivedMessage.ChatId, "Обновили контекст");
            return this;
        }

        if (receivedMessage.Content is not TextContent text)
        {
            await messenger.Send(receivedMessage.ChatId, "Поддерживаются только текстовые сообщения");
            return this;
        }

        try
        {
            string chatId = receivedMessage.ChatId.ToString();
            User user = (await _botRepository.GetUserById(chatId))!;
            Conversation conversation = GetOrCreateConversation(chatId);

            await RefreshContextIfTooLarge(receivedMessage.ChatId, conversation, text.Content, messenger);

            conversation.AppendUserInput(text.Content);
            MessageId id = await messenger.Send(receivedMessage.ChatId,
                new SendInfo(new TextContent("Отправляем боту...")) {HideNotification = true});

            try
            {
                string response;
                if (user.AnswerType is BotAnswerType.ByPart)
                    response = await ProcessQueryWithFastAnswer(messenger, conversation, receivedMessage.ChatId);
                else
                    response = await ProcessQueryWithFullAnswer(messenger, conversation, receivedMessage.ChatId);

                _logger.LogDebug("Пользователь {@User} спросил: {Question}, ответ: {Answer}", receivedMessage.From,
                    text.Content, response);
            }
            finally
            {
                await messenger.Delete(receivedMessage.ChatId, id);
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Rate limit reached"))
            {
                _logger.LogWarning("Лимит запросов достигнут, сообщение от {@User} не может быть обработано",
                    receivedMessage.From);
                await messenger.Send(receivedMessage.ChatId, "Лимит запросов, подожди следующей минуты");
            }
            else
            {
                _logger.LogError(ex, "Ошибка при обработке сообщения от пользователя {@User}", receivedMessage.From);
                await messenger.Send(receivedMessage.ChatId, $"Произошла ошибка: {ex.Message}");
            }
        }

        return this;
    }

    private async Task<string> ProcessQueryWithFullAnswer(IMessenger messenger, Conversation conversation,
        ChatId chatId)
    {
        string? response = await conversation.GetResponseFromChatbotAsync();
        await messenger.Send(chatId, response ?? "нет ответа :(");
        return response ?? string.Empty;
    }

    private async Task<string> ProcessQueryWithFastAnswer(IMessenger messenger, Conversation conversation,
        ChatId chatId)
    {
        await using var sender = new PartAnswerSender(messenger, conversation, chatId, _partSenderLogger);

        await conversation.StreamResponseFromChatbotAsync(sender.OnNewMessage);
        await sender.State;

        if (sender.MessageId != null)
            await messenger.Edit(chatId, sender.MessageId, sender.Message);
        else
            await messenger.Send(chatId, sender.Message);

        return sender.Message;
    }
}
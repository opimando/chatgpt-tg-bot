#region Copyright

/*
 * File: HelpCommand.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using TgBotFramework.Core;

namespace ChatGptBot.Commands;

[TelegramState("/help")]
public class HelpCommand : BaseChatState
{
    private readonly IChatStateFactory _stateFactory;

    public HelpCommand(IChatStateFactory stateFactory, IEventBus bus) : base(bus)
    {
        _stateFactory = stateFactory;
    }

    private const string HELP_MESSAGE = @"Все сообщения кроме команд отправляются в OpenAi. 
Модель используется gpt-3.5-turbo.

Максимальное количество символов от тебя в рамках одного контекста - 4к.
Если понимаешь что тебе надо спросить несколько вопросов в одном контексте, то предварительно сделай /refresh и контекст обновится.

В настройках /settings можно изменить способ ответа: подождать полный ответ и прислать его; присылать по мере ответа гпт ботом.
Не забудь Завершить настройки.

Ограничение бесплатного openAI - 3 запроса в минуту на всех пользователей бота. 
Хостится на домашнем ПК и бот в оффлайне - это норма, но бывает редко.

Если стабильно падает ошибка пиши @opimand
";

    protected override async Task<IChatState?> InternalProcessMessage(Message receivedMessage, IMessenger messenger)
    {
        await messenger.Send(receivedMessage.ChatId, HELP_MESSAGE);
        return await _stateFactory.CreateState<ChatCommand>();
    }
}
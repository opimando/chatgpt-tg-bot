#region Copyright

/*
 * File: StartCommand.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using TgBotFramework.Core;

namespace ChatGptBot.Commands;

[TelegramState("/start")]
public class StartCommand : BaseChatState
{
    private readonly IChatStateFactory _stateFactory;
    private readonly IAdminAccessResolver _adminResolver;

    public StartCommand(IEventBus eventsBus, IChatStateFactory stateFactory, IAdminAccessResolver adminResolver) :
        base(eventsBus)
    {
        _stateFactory = stateFactory;
        _adminResolver = adminResolver;
    }

    protected override async Task<IChatState?> InternalProcessMessage(Message receivedMessage, IMessenger messenger)
    {
        bool isAdmin = await _adminResolver.IsAdmin(receivedMessage.From.Id);

        KeyboardButtonGroup? buttons =
            isAdmin
                ? new KeyboardButtonGroup(new[] {new KeyboardButton("/adduser"), new KeyboardButton("/deleteuser")})
                : null;

        await messenger.Send(receivedMessage.ChatId,
            new SendInfo(
                new TextContent("Для информации можешь вызвать /help а для обращения к GTP просто пиши текст"))
            {
                Buttons = buttons
            });

        return await _stateFactory.CreateState<ChatCommand>();
    }
}
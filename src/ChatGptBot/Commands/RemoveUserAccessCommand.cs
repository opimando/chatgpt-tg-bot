#region Copyright

/*
 * File: RemoveUserAccessCommand.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel;
using TgBotFramework.Core;

namespace ChatGptBot.Commands;

[TelegramState("/deleteuser")]
public class RemoveUserAccessCommand : AddAccessToUserCommand
{
    public RemoveUserAccessCommand(
        IBotRepository repository,
        IEventBus eventsBus,
        IAdminAccessResolver adminAccessResolver,
        IChatStateFactory stateFactory) : base(eventsBus, repository, stateFactory, adminAccessResolver)
    {
    }

    protected override string GetStartMessage()
    {
        return "Перешли сообщение от пользователя, у которого хочешь забрать доступ";
    }

    protected override async Task<IChatState?> InternalProcessMessage(Message receivedMessage, IMessenger messenger)
    {
        if (IsFirstStateInvoke) return this;
        if (!await AdminAccessResolver.IsAdmin(receivedMessage.From.Id)) return null;

        if (receivedMessage.ForwardedFrom == null)
        {
            await messenger.Send(receivedMessage.ChatId, "Не найден аккаунт, с которого переслано сообщение");
            return await StateFactory.CreateState<ChatCommand>();
        }

        await Repository.SetUserAccess(receivedMessage.ForwardedFrom.Id.ToString(), false);
        await messenger.Send(receivedMessage.ChatId, "Доступ деактивирован");
        return await StateFactory.CreateState<ChatCommand>();
    }
}
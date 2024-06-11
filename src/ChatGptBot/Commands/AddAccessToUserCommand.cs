#region Copyright

/*
 * File: AddAccessToUserCommand.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel;
using TgBotFramework.Core;
using User = Bot.DbModel.Models.User;

namespace ChatGptBot.Commands;

[TelegramState("/adduser")]
public class AddAccessToUserCommand : BaseChatState
{
    protected readonly IBotRepository Repository;
    protected readonly IChatStateFactory StateFactory;
    protected readonly IAdminAccessResolver AdminAccessResolver;

    public AddAccessToUserCommand(
        IEventBus eventsBus,
        IBotRepository repository,
        IChatStateFactory stateFactory,
        IAdminAccessResolver adminAccessResolver
    ) : base(eventsBus)
    {
        Repository = repository;
        StateFactory = stateFactory;
        AdminAccessResolver = adminAccessResolver;
    }

    protected virtual string GetStartMessage()
    {
        return "Перешли сообщение от пользователя, которому хочешь дать доступ";
    }

    protected override async Task OnStateStartInternal(IMessenger messenger, ChatId chatId)
    {
        if (!await AdminAccessResolver.IsAdmin(new UserId(chatId.Id))) throw new Exception("Нет доступа");
        await messenger.Send(chatId, GetStartMessage());
    }

    protected override async Task<IChatState?> InternalProcessMessage(Message receivedMessage, IMessenger messenger)
    {
        if (IsFirstStateInvoke) return this;
        if (!await AdminAccessResolver.IsAdmin(receivedMessage.From.Id)) return null;

        if (receivedMessage.ForwardedFrom == null)
        {
            await messenger.Send(receivedMessage.ChatId, "Не найден аккаунт, с которого переслано сообщение");
            return this;
        }

        User? user = await Repository.GetUserById(receivedMessage.ForwardedFrom.Id.ToString());
        if (user == null)
        {
            var newUser = new User
            {
                Id = receivedMessage.ForwardedFrom.Id.ToString(),
                CreateTime = DateTime.Now,
                FullName = receivedMessage.ForwardedFrom.FriendlyName,
                UserName = receivedMessage.ForwardedFrom.UserName,
                HasAccess = true,
                LastAccessTime = DateTime.Now
            };
            await Repository.AddUser(newUser);
            await messenger.Send(receivedMessage.ChatId, "Доступ выдан");
            return await StateFactory.CreateState<ChatCommand>();
        }

        await Repository.SetUserAccess(user.Id, true);
        await messenger.Send(receivedMessage.ChatId, "Доступ выдан");
        return await StateFactory.CreateState<ChatCommand>();
    }
}
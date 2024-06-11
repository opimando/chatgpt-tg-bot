#region Copyright

/*
 * File: IAdminAccessResolver.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel;
using Bot.DbModel.Models;
using TgBotFramework.Core;
using User = Bot.DbModel.Models.User;

namespace ChatGptBot;

public interface IAdminAccessResolver
{
    Task<bool> IsAdmin(UserId chatId);
}

public class AdminAccessResolver : IAdminAccessResolver
{
    private readonly IBotRepository _repository;

    public AdminAccessResolver(IBotRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> IsAdmin(UserId chatId)
    {
        User? localUser = await _repository.GetUserById(chatId.ToString());
        return localUser is {HasAccess: true, UserType: UserType.Admin};
    }
}
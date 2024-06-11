#region Copyright

/*
 * File: UserAuthAccessProvider.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel;
using TgBotFramework.Core;
using User = Bot.DbModel.Models.User;

namespace ChatGptBot;

public class UserAuthAccessProvider : IAuthProvider
{
    private readonly IBotRepository _botRepository;

    public UserAuthAccessProvider(IBotRepository botRepository)
    {
        _botRepository = botRepository;
    }

    private async Task AddUser(ChatId chatId, string fullName, string userName)
    {
        await _botRepository.AddUser(new User
        {
            Id = chatId.ToString(),
            FullName = fullName,
            UserName = userName,
            CreateTime = DateTime.Now,
            LastAccessTime = DateTime.Now
        });
    }

    public async Task<bool> HasAccess(TgBotFramework.Core.User user)
    {
        User? localUser = await _botRepository.GetUserById(user.Id.ToString());
        if (localUser == null)
        {
            await AddUser(user.Id, user.FriendlyName, user.UserName);
            return false;
        }

        await _botRepository.UpdateUserLastActivity(user.Id.ToString());
        return localUser.HasAccess;
    }

    public Task<string> GetAccessDeniedMessage()
    {
        return Task.FromResult("Нет доступа :(");
    }
}
#region Copyright

/*
 * File: IBotRepository.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel.Models;

namespace Bot.DbModel;

public interface IBotRepository
{
    Task<User?> GetUserById(string userId);
    Task AddUser(User user);
    Task UpdateAnswerType(string userId, BotAnswerType type);
    Task SetUserAccess(string userId, bool hasAccess);
    Task UpdateUserLastActivity(string userId);
    Task<int> GetActiveUsersCount();
}
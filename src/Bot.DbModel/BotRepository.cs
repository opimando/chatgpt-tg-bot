#region Copyright

/*
 * File: BotRepository.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel.Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.DbModel;

public class BotRepository : IBotRepository
{
    private readonly IDbContextFactory<BotDbContext> _dbFactory;

    public BotRepository(IDbContextFactory<BotDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<User?> GetUserById(string userId)
    {
        await using BotDbContext context = await _dbFactory.CreateDbContextAsync();
        return await context.Users.FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task AddUser(User user)
    {
        await using BotDbContext context = await _dbFactory.CreateDbContextAsync();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAnswerType(string userId, BotAnswerType type)
    {
        User? user = await GetUserById(userId);
        if (user == null) throw new Exception($"Не удалось найти пользователья с идентификатором {userId}");
        await using BotDbContext context = await _dbFactory.CreateDbContextAsync();
        var entity = context.Entry(user);
        entity.State = EntityState.Modified;
        entity.Entity.AnswerType = type;
        await context.SaveChangesAsync();
    }

    public async Task SetUserAccess(string userId, bool hasAccess)
    {
        User? user = await GetUserById(userId);
        if (user == null) throw new Exception($"Не удалось найти пользователья с идентификатором {userId}");
        await using BotDbContext context = await _dbFactory.CreateDbContextAsync();
        var entity = context.Entry(user);
        entity.State = EntityState.Modified;
        entity.Entity.HasAccess = hasAccess;
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserLastActivity(string userId)
    {
        User? user = await GetUserById(userId);
        if (user == null) throw new Exception($"Не удалось найти пользователья с идентификатором {userId}");
        await using BotDbContext context = await _dbFactory.CreateDbContextAsync();
        var entity = context.Entry(user);
        entity.State = EntityState.Modified;
        entity.Entity.LastAccessTime = DateTime.Now;
        await context.SaveChangesAsync();
    }

    public async Task<int> GetActiveUsersCount()
    {
        await using BotDbContext context = await _dbFactory.CreateDbContextAsync();
        return await context.Users.CountAsync(user => user.HasAccess);
    }
}
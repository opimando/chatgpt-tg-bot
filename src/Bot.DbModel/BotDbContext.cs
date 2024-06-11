#region Copyright

/*
 * File: BotDbContext.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel.Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.DbModel;

public class BotDbContext : DbContext
{
    public BotDbContext()
    {
    }

    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(p => p.CreateTime).HasConversion(
            runtimeToDb => runtimeToDb.ToUniversalTime(),
            dbToRuntime => dbToRuntime.ToLocalTime()
        );
        modelBuilder.Entity<User>().Property(p => p.LastAccessTime).HasConversion(
            runtimeToDb => runtimeToDb.ToUniversalTime(),
            dbToRuntime => dbToRuntime.ToLocalTime()
        );

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            base.OnConfiguring(optionsBuilder);
            return;
        }

        string connectionString =
            "Server=localhost;port=5432;Database=gptbotmigrations;User Id=postgres;Password=123456";
        optionsBuilder.UseNpgsql(connectionString);
    }
}
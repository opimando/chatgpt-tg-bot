#region Copyright

/*
 * File: DbSettings.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

namespace ChatGptBot.Settings;

public class DbSettings
{
    public string ConnectionString { get; set; } =
        "Server=localhost;port=5432;Database=gptbotmigrations;User Id=postgres;Password=123456";
}
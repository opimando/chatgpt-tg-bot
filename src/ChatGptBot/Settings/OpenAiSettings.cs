#region Copyright

/*
 * File: OpenAiSettings.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

namespace ChatGptBot.Settings;

public class OpenAiSettings
{
    public string? OpenAiToken { get; set; }
    public int MaxContextLength { get; set; } = 4000;
}
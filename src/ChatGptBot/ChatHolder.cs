#region Copyright

/*
 * File: ChatHolder.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using System.Collections.Concurrent;
using OpenAI_API.Chat;

namespace ChatGptBot;

public class ChatHolder
{
    private readonly ConcurrentDictionary<string, Conversation> _userConversations = new();

    public Conversation? Get(string id)
    {
        if (_userConversations.TryGetValue(id, out Conversation? conv))
            return conv;

        return null;
    }

    public void AddOrUpdate(string id, Conversation conv)
    {
        _userConversations.AddOrUpdate(
            id,
            _ => conv,
            (_, _) => conv
        );
    }
}
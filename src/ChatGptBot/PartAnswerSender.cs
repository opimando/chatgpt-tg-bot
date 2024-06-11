#region Copyright

/*
 * File: PartAnswerSender.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using System.Text;
using Microsoft.Extensions.Logging;
using OpenAI_API.Chat;
using TgBotFramework.Core;

namespace ChatGptBot;

public class PartAnswerSender : IDisposable, IAsyncDisposable
{
    private readonly IMessenger _messenger;
    private readonly Conversation _conversation;
    private int _lastSendCount;
    private readonly ChatId _chatId;
    private readonly StringBuilder _message = new();
    private readonly TaskCompletionSource _source = new();
    private readonly Timer _timer;
    private readonly ILogger<PartAnswerSender> _logger;
    public MessageId? MessageId { get; private set; }
    private readonly int _messagesCount;

    public PartAnswerSender(
        IMessenger messenger,
        Conversation conversation,
        ChatId chatId,
        ILogger<PartAnswerSender> logger
    )
    {
        _messagesCount = conversation.Messages.Count;
        _messenger = messenger;
        _conversation = conversation;
        _chatId = chatId;
        _logger = logger;
        _timer = new Timer(Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void Callback(object? state)
    {
        Task.Run(async () =>
        {
            try
            {
                if (_messagesCount < _conversation.Messages.Count)
                {
                    _source.TrySetResult();
                    return;
                }

                if (_message.Length == _lastSendCount) return;
                if (MessageId == null)
                    MessageId = await _messenger.Send(_chatId, $"{_message}...✏️");
                else
                    await _messenger.Edit(_chatId, MessageId, $"{_message}...✏️");

                _lastSendCount = _message.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке/обновлении частичного ответа");
            }
        });
    }

    public Task State => _source.Task;

    public void OnNewMessage(string text)
    {
        _logger.LogDebug("Добавляем часть ответа для пользователя {UserId}", _chatId);
        _message.Append(text);
    }

    public string Message => _message.ToString();

    public void Dispose()
    {
        _timer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
    }
}
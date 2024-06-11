#region Copyright

/*
 * File: OptionsCommand.cs
 * Author: denisosipenko
 * Created: 2023-05-28
 * Copyright © 2023 Denis Osipenko
 */

#endregion Copyright

using Bot.DbModel;
using Bot.DbModel.Models;
using TgBotFramework.Core;
using User = Bot.DbModel.Models.User;

namespace ChatGptBot.Commands;

[TelegramState("/settings")]
public class OptionsCommand : BaseChatState, IChatStateWithData<OptionsData>
{
    private readonly IBotRepository _botRepository;
    private readonly IChatStateFactory _stateFactory;
    private OptionsData _data = new();
    private const string SaveCommand = "save";
    private const string PartButtonId = "part";
    private const string FullButtonId = "full";

    public OptionsCommand(IBotRepository botRepository, IChatStateFactory stateFactory, IEventBus eventsBus) :
        base(eventsBus)
    {
        _botRepository = botRepository;
        _stateFactory = stateFactory;
    }

    protected override async Task OnStateStartInternal(IMessenger messenger, ChatId chatId)
    {
        User user = (await _botRepository.GetUserById(chatId.ToString()))!;
        _data.SettingsButtons = new MessageWithSelectableItems(new TextContent(
            "В настройках ты можешь изменить способ ответа от бота. Либо ждать полный ответ, либо получать ответ постепенно по мере готовности."))
        {
            Items = new List<SelectableInlineItem>
            {
                new("По готовности", PartButtonId) {IsSelected = user.AnswerType is BotAnswerType.ByPart},
                new("Целиком", FullButtonId) {IsSelected = user.AnswerType is BotAnswerType.Full}
            }
        };
        await _data.SettingsButtons.AddOrUpdate(messenger, chatId);
        _data.Add(await messenger.Send(chatId,
            new SendInfo(new TextContent("Когда выберешь нажми 'Сохранить'"))
                {Buttons = new InlineButtonGroup(new[] {new InlineButton("Сохранить", SaveCommand)})}));
        _data.Add(_data.SettingsButtons.MessageId!);
    }

    protected override async Task OnStateExitInternal(IMessenger messenger, ChatId chatId)
    {
        foreach (MessageId message in _data.MessagesIds) await messenger.Delete(chatId, message);
        _data.MessagesIds.Clear();
    }

    protected override async Task<IChatState?> InternalProcessMessage(Message receivedMessage, IMessenger messenger)
    {
        if (receivedMessage.Content is not CallbackInlineButtonContent) _data.Add(receivedMessage.Id);
        if (receivedMessage.Content is not CallbackInlineButtonContent callback ||
            string.IsNullOrWhiteSpace(callback.Data)) return this;

        SelectableInlineItem fullItem = _data.SettingsButtons!.Items.First(s => s.Id == FullButtonId);
        if (callback.Data == SaveCommand)
        {
            await _botRepository.UpdateAnswerType(receivedMessage.ChatId.ToString(),
                fullItem.IsSelected ? BotAnswerType.Full : BotAnswerType.ByPart);
            return await _stateFactory.CreateState<ChatCommand>();
        }

        bool isFull = callback.Data == FullButtonId;
        SelectableInlineItem partItem = _data.SettingsButtons.Items.First(s => s.Id == PartButtonId);
        if ((fullItem.IsSelected && isFull) || (partItem.IsSelected && isFull == false)) return this;
        fullItem.IsSelected = isFull;
        partItem.IsSelected = !isFull;
        await _data.SettingsButtons.AddOrUpdate(messenger, receivedMessage.ChatId);

        return this;
    }

    public Task SetData(OptionsData data)
    {
        _data = data;
        return Task.CompletedTask;
    }

    public OptionsData GetData()
    {
        return _data;
    }
}

public class OptionsData : MessageToDeleteArgument
{
    public MessageWithSelectableItems? SettingsButtons { get; set; }
}
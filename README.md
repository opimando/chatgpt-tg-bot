# chatgptbot

This is a Telegram bot built with .NET 6 that serves as a provider for the ChatGPT API. The bot allows users to interact with OpenAI's ChatGPT directly from their Telegram app. The project includes Docker support with a `Dockerfile` and `docker-compose.yml` for easy deployment.

### Getting Started

Follow these steps to set up and run the project on your local machine for development and testing purposes.
The GPT model can be configured in the `src/ChatGptBot/Commands/ChatCommand.cs` file within the `GetOrCreateConversation` method.

1. Clone the repository:
   ```sh
   git clone https://github.com/opimando/chatgpt-tg-bot.git
   cd chatgpt-tg-bot
   ```
2. Build the Docker image.
3. Copy the docker-compose.yml file located at src/docker-compose.yml to your project root or preferred directory.
4. Update the docker-compose.yml file to include your API keys:
 ```dyaml
services:
  chatgptbot:
    image: chatgptbot
    environment:
      - ApiKey=your-telegram-bot-token
      - OpenAiToken=your-openai-api-key
      - ConnectionString=Server=db;port=5432;Database=chatgpt;User Id=postgres;Password=123456
```

### Contributing

Contributions are welcome! Please open an issue or submit a pull request.

### Acknowledgments

* [TgBotFramework](https://github.com/opimando/TelegramBotFramework)
* [openai-dotnet](https://github.com/openai/openai-dotnet/tree/OpenAI_2.0.0-beta.4)
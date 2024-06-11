FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChatGptBot/ChatGptBot.csproj", "ChatGptBot/"]
COPY ["Bot.DbModel/Bot.DbModel.csproj", "Bot.DbModel/"]
RUN dotnet restore "ChatGptBot/ChatGptBot.csproj"
COPY . .
WORKDIR "/src/ChatGptBot"
RUN dotnet build "ChatGptBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ChatGptBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatGptBot.dll"]

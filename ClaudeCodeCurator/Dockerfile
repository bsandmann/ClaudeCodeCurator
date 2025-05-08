FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 3330

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClaudeCodeCurator/ClaudeCodeCurator.csproj", "ClaudeCodeCurator/"]
RUN dotnet restore "ClaudeCodeCurator/ClaudeCodeCurator.csproj"
COPY . .
WORKDIR "/src/ClaudeCodeCurator"
RUN dotnet build "./ClaudeCodeCurator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClaudeCodeCurator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClaudeCodeCurator.dll"]

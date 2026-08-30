# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DungeonRush.Api/DungeonRush.Api.csproj DungeonRush.Api/
COPY DungeonRush.Shared/DungeonRush.Shared.csproj DungeonRush.Shared/
RUN dotnet restore DungeonRush.Api/DungeonRush.Api.csproj

COPY DungeonRush.Api/ DungeonRush.Api/
COPY DungeonRush.Shared/ DungeonRush.Shared/
RUN dotnet publish DungeonRush.Api/DungeonRush.Api.csproj -c Release -o /app --no-restore

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Запуск не от root — снижает ущерб при компрометации контейнера
RUN useradd -m appuser
USER appuser

COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DungeonRush.Api.dll"]

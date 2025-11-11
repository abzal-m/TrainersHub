# 1. Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0.100 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# 2. Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:10000

# подключение БД через переменные окружения
ENV ASPNETCORE_URLS=http://+:10000
ENV ConnectionStrings__DefaultConnection="Host=dpg-d49nfi0dl3ps739b84gg-a.render.com;Port=5432;Database=trainershubdb_yifq;Username=trainershubdb_yifq_user;Password=GCkH1qVOHrPWVDBDYprmFHIjr321oxx4;Ssl Mode=Require;Trust Server Certificate=true"

EXPOSE 10000
ENTRYPOINT ["dotnet", "TrainersHub.dll"]
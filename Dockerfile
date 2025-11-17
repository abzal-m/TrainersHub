# ================================
# 1. BUILD STAGE
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем проекты (если монорепозиторий — скорректируй)
COPY . .

# Восстановление пакетов
RUN dotnet restore

# Публикация в Release
RUN dotnet publish -c Release -o /app


# ================================
# 2. RUNTIME STAGE
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем готовую сборку
COPY --from=build /app .

# Render пробрасывает порт через ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:10000

# ================================
# 3. Переменные окружения приложения
# (в продакшене Render все будет переопределено,
# но оставляем default-значения, чтобы локально работало)
# ================================

# Подключение к БД — Render все равно заменит это своим значением
ENV ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=trainershub;Username=postgres;Password=password;Ssl Mode=Disable"

# JWT (локальные значения — в Render обязательно переопределить!)
ENV Jwt__Key="local_dev_jwt_key_1234567890_very_secret_123456"
ENV Jwt__Issuer="JwtAuthExample"
ENV Jwt__Audience="JwtAuthExampleUsers"
ENV Jwt__AccessTokenExpireMinutes=15
ENV Jwt__RefreshTokenExpireDays=7

# CORS (можно переопределить в Render)
ENV CORS__Origins="http://localhost:3000"

# Открываем порт
EXPOSE 10000

# Запуск приложения
ENTRYPOINT ["dotnet", "TrainersHub.dll"]

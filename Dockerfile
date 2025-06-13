# 使用官方 .NET 8 SDK 映像進行建置
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製專案檔案並還原 NuGet 套件
COPY *.csproj ./
RUN dotnet restore

# 複製所有原始檔案並建置應用程式
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# 使用輕量的 runtime 映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 安裝必要工具
RUN apt-get update && \
    apt-get install -y curl default-mysql-client && \
    rm -rf /var/lib/apt/lists/*

# 建立 keys 目錄解決 DataProtection 警告
RUN mkdir -p /tmp/keys && chmod 755 /tmp/keys

# 複製發布的應用程式
COPY --from=build /app/publish .

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# 預設資料庫連線字串（可透過環境變數覆蓋）
ENV CONNECTION_STRING="Server=host.docker.internal;Database=DipDb;User=root;Password=password;Port=3306;"

# 建立非 root 用戶
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app && \
    chown -R appuser:appuser /tmp/keys
USER appuser

# 健康檢查 - 包含資料庫連線檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost/health || curl -f http://localhost/ping || exit 1

# 暴露端口
EXPOSE 80

# 應用程式入口點
ENTRYPOINT ["dotnet", "DIP.dll"]
# 使用官方的 .NET runtime 作為基礎映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 使用 SDK 映像進行建置
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DIP.csproj", "."]
RUN dotnet restore "./DIP.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DIP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DIP.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DIP.dll"]

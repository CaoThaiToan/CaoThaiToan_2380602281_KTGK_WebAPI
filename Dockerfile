FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CaoThaiToan_2380602281_KTGK_WebAPI/CaoThaiToan_2380602281_KTGK_WebAPI.csproj", "CaoThaiToan_2380602281_KTGK_WebAPI/"]
RUN dotnet restore "./CaoThaiToan_2380602281_KTGK_WebAPI/CaoThaiToan_2380602281_KTGK_WebAPI.csproj"
COPY . .
WORKDIR "/src/CaoThaiToan_2380602281_KTGK_WebAPI"
RUN dotnet build "./CaoThaiToan_2380602281_KTGK_WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CaoThaiToan_2380602281_KTGK_WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CaoThaiToan_2380602281_KTGK_WebAPI.dll"]

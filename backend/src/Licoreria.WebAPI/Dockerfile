# syntax=docker/dockerfile:1

# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/Licoreria.WebAPI/Licoreria.WebAPI.csproj -r linux-musl-x64
RUN dotnet publish src/Licoreria.WebAPI/Licoreria.WebAPI.csproj \
    -c Release -r linux-musl-x64 --self-contained false -o /app/publish /p:UseAppHost=false

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
# Alpine: sin ICU (cultura invariante) y con libs nativas para QuestPDF/Skia.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
RUN apk add --no-cache fontconfig freetype
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Licoreria.WebAPI.dll"]

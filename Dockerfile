# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config ./
COPY src/CatalogAPI/CatalogAPI.csproj src/CatalogAPI/
RUN dotnet restore src/CatalogAPI/CatalogAPI.csproj

COPY . .
RUN dotnet publish src/CatalogAPI/CatalogAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CatalogAPI.dll"]

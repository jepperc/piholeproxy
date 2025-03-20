# Build-stage: Brug .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopier kun CSharpProxy.csproj fra src/Proxy (testprojektet kommer ikke med)
COPY src/Proxy/CSharpProxy.csproj ./
# Genskab afhængigheder
RUN dotnet restore CSharpProxy.csproj

# Kopier resten af kildekoden fra src/Proxy (ikke src/Proxy.Test)
COPY src/Proxy/. ./

# Publish projektet i Release-konfiguration
RUN dotnet publish CSharpProxy.csproj -c Release -o /app/publish --no-restore

# Runtime-stage: Brug .NET 9 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "CSharpProxy.dll"]

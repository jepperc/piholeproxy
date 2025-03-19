# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src/Proxy/
COPY *.csproj ./
RUN pwd && ls -l
RUN dotnet restore /src/Proxy/CSharpProxy.csproj
COPY . .
RUN pwd && ls -l
RUN dotnet publish /src/Proxy/CSharpProxy.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "CSharpProxy.dll"]

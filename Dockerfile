# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src/Proxy/
COPY *.csproj ./
#RUN dotnet restore CSharpProxy.csproj
COPY . .
RUN pwd && ls -l
RUN dotnet publish CSharpProxy.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "CSharpProxy.dll"]

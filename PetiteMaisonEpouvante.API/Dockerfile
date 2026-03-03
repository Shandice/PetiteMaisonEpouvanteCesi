# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Collector.API/Collector.API.csproj", "Collector.API/"]
COPY ["Collector.Core/Collector.Core.csproj", "Collector.Core/"]
RUN dotnet restore "Collector.API/Collector.API.csproj"
COPY . .
WORKDIR "/src/Collector.API"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Collector.API.dll"]
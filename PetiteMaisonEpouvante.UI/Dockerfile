# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Collector.UI/Collector.UI.csproj", "Collector.UI/"]
COPY ["Collector.Core/Collector.Core.csproj", "Collector.Core/"]
RUN dotnet restore "Collector.UI/Collector.UI.csproj"
COPY . .
WORKDIR "/src/Collector.UI"
RUN dotnet publish -c Release -o /app/publish

# Run Stage (Nginx)
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY Collector.UI/nginx.conf /etc/nginx/conf.d/default.conf
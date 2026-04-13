# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PetiteMaisonEpouvante.UI/PetiteMaisonEpouvante.UI.csproj", "PetiteMaisonEpouvante.UI/"]
COPY ["PetiteMaisonEpouvante.Core/PetiteMaisonEpouvante.Core.csproj", "PetiteMaisonEpouvante.Core/"]
RUN dotnet restore "PetiteMaisonEpouvante.UI/PetiteMaisonEpouvante.UI.csproj"
COPY . .
WORKDIR "/src/PetiteMaisonEpouvante.UI"
RUN dotnet publish -c Release -o /app/publish

# Run Stage (Nginx)
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY PetiteMaisonEpouvante.UI/nginx.conf /etc/nginx/conf.d/default.conf
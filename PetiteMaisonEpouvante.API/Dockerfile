# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PetiteMaisonEpouvante.API/PetiteMaisonEpouvante.API.csproj", "PetiteMaisonEpouvante.API/"]
COPY ["PetiteMaisonEpouvante.Core/PetiteMaisonEpouvante.Core.csproj", "PetiteMaisonEpouvante.Core/"]
RUN dotnet restore "PetiteMaisonEpouvante.API/PetiteMaisonEpouvante.API.csproj"
COPY . .
WORKDIR "/src/PetiteMaisonEpouvante.API"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "PetiteMaisonEpouvante.API.dll"]
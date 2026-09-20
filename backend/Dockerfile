# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY backend/DartCupBackend.csproj ./
COPY backend/*.cs ./
COPY backend/Controllers/*.cs ./Controllers/
COPY backend/Services/*.cs ./Services/
COPY backend/Models/*.cs ./Models/
COPY backend/Database/*.cs ./Database/

# Restore dependencies
RUN dotnet restore "DartCupBackend.csproj"

# Build the application
RUN dotnet build "DartCupBackend.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DartCupBackend.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Copy appsettings
COPY backend/appsettings.json ./
COPY backend/appsettings.Development.json ./

# Set entry point
ENTRYPOINT ["dotnet", "DartCupBackend.dll"]

# Expose ports
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["MonolithApp.csproj", "."]
RUN dotnet restore "MonolithApp.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "MonolithApp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MonolithApp.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Create data directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

# Expose ports
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "MonolithApp.dll"]

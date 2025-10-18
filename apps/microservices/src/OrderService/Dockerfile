# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MicroservicesArchitecture.sln", "./"]
COPY ["src/OrderService/OrderService.csproj", "src/OrderService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

# Restore dependencies
RUN dotnet restore "src/OrderService/OrderService.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/src/OrderService"
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for SQLite database
RUN mkdir -p /data

ENTRYPOINT ["dotnet", "OrderService.dll"]


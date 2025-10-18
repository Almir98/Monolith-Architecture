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
COPY ["src/BulkService/BulkService.csproj", "src/BulkService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

# Restore dependencies
RUN dotnet restore "src/BulkService/BulkService.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/src/BulkService"
RUN dotnet build "BulkService.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "BulkService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BulkService.dll"]


# Dockerfile.api
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy only the project files first to leverage layer caching.
COPY ["src/minitwit.Api/minitwit.Api.csproj", "minitwit.Api/"]
COPY ["src/minitwit.Application/minitwit.Application.csproj", "minitwit.Application/"]
COPY ["src/minitwit.Domain/minitwit.Domain.csproj", "minitwit.Domain/"]
COPY ["src/minitwit.Infrastructure/minitwit.Infrastructure.csproj", "minitwit.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "minitwit.Api/minitwit.Api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/minitwit.Api"
RUN dotnet build "minitwit.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "minitwit.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "minitwit.Api.dll"]

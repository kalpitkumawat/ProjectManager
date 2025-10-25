# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ProjectManager.Api/*.csproj ./ProjectManager.Api/
RUN dotnet restore "./ProjectManager.Api/ProjectManager.Api.csproj"

# Copy everything else and build
COPY ProjectManager.Api/ ./ProjectManager.Api/
WORKDIR "/src/ProjectManager.Api"
RUN dotnet publish "ProjectManager.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectManager.Api.dll"]

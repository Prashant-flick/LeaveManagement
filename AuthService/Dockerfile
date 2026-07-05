FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files for cache optimization during restores
COPY Auth.API/Auth.API.csproj Auth.API/
COPY Auth.Application/Auth.Application.csproj Auth.Application/
COPY Auth.Domain/Auth.Domain.csproj Auth.Domain/
COPY Auth.Infrastructure/Auth.Infrastructure.csproj Auth.Infrastructure/

# Restore NuGet dependencies
RUN dotnet restore Auth.API/Auth.API.csproj

# Copy remaining source code files
COPY . .

# Build and publish release binaries
WORKDIR /src/Auth.API
RUN dotnet publish Auth.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime image container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Auth.API.dll"]

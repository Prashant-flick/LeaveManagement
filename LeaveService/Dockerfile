FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files for cache optimization during restores
COPY Leave.API/Leave.API.csproj Leave.API/
COPY Leave.Application/Leave.Application.csproj Leave.Application/
COPY Leave.Domain/Leave.Domain.csproj Leave.Domain/
COPY Leave.Infrastructure/Leave.Infrastructure.csproj Leave.Infrastructure/

# Restore NuGet dependencies
RUN dotnet restore Leave.API/Leave.API.csproj

# Copy remaining source code files
COPY . .

# Build and publish release binaries
WORKDIR /src/Leave.API
RUN dotnet publish Leave.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime image container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Leave.API.dll"]

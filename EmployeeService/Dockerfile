FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files for cache optimization during restores
COPY Employee.API/Employee.API.csproj Employee.API/
COPY Employee.Application/Employee.Application.csproj Employee.Application/
COPY Employee.Domain/Employee.Domain.csproj Employee.Domain/
COPY Employee.Infrastructure/Employee.Infrastructure.csproj Employee.Infrastructure/

# Restore NuGet dependencies
RUN dotnet restore Employee.API/Employee.API.csproj

# Copy remaining source code files
COPY . .

# Build and publish release binaries
WORKDIR /src/Employee.API
RUN dotnet publish Employee.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime image container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Employee.API.dll"]

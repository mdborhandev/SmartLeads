# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ../../ContactManager.slnx ./
COPY ../src.sln ./
COPY ../ContactManager.Domain/ContactManager.Domain.csproj ../ContactManager.Domain/
COPY ../ContactManager.Application/ContactManager.Application.csproj ../ContactManager.Application/
COPY ../ContactManager.Infrastructure/ContactManager.Infrastructure.csproj ../ContactManager.Infrastructure/
COPY ../ContactManager.Web/ContactManager.Web.csproj ../ContactManager.Web/

# Restore dependencies
RUN dotnet restore ./ContactManager.Web/ContactManager.Web.csproj

# Copy source code
COPY ../ContactManager.Domain/ ../ContactManager.Domain/
COPY ../ContactManager.Application/ ../ContactManager.Application/
COPY ../ContactManager.Infrastructure/ ../ContactManager.Infrastructure/
COPY ../ContactManager.Web/ ../ContactManager.Web/

# Build and publish
WORKDIR /src/ContactManager.Web
RUN dotnet publish ./ContactManager.Web.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 10000

ENV ASPNETCORE_URLS=http://+:10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ContactManager.Web.dll"]

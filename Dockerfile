# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy solution and project files
COPY VisaSponsorshipScout.sln ./
COPY VisaSponsorshipScout.API/*.csproj ./VisaSponsorshipScout.API/
COPY VisaSponsorshipScout.Application/*.csproj ./VisaSponsorshipScout.Application/
COPY VisaSponsorshipScout.Core/*.csproj ./VisaSponsorshipScout.Core/
COPY VisaSponsorshipScout.Infrastructure/*.csproj ./VisaSponsorshipScout.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY VisaSponsorshipScout.API/. ./VisaSponsorshipScout.API/
COPY VisaSponsorshipScout.Application/. ./VisaSponsorshipScout.Application/
COPY VisaSponsorshipScout.Core/. ./VisaSponsorshipScout.Core/
COPY VisaSponsorshipScout.Infrastructure/. ./VisaSponsorshipScout.Infrastructure/

# Build and publish
RUN dotnet publish VisaSponsorshipScout.API/VisaSponsorshipScout.API.csproj -c Release -o out

# Stage 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Use port
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "VisaSponsorshipScout.API.dll"]
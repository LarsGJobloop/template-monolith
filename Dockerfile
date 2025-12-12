# =======================================
# = Dockerfile for the application =
# =======================================

# ===============
# = Build stage =
# ===============

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution file
COPY *.slnx ./

# Copy all project files for dependency resolution
COPY src/ ./src/

# Restore dependencies
RUN dotnet restore src/Application/*.csproj

# Build and publish the specific service
WORKDIR /app/src/Application
RUN dotnet publish --configuration Release --output /app/publish /p:UseAppHost=false



# =================
# = Runtime stage =
# =================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Set Ports kestrel listens on
ENV HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# When the container starts, run this command
ENTRYPOINT ["sh", "-c", "dotnet Application.dll"]

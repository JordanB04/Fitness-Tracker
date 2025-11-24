# ======================
# 1. Build Stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file first
COPY FitnessTracker.Api.csproj ./

# Restore dependencies
RUN dotnet restore "FitnessTracker.Api.csproj"

# Copy the rest of the project
COPY . .

# Publish
RUN dotnet publish "FitnessTracker.Api.csproj" -c Release -o /app/out

# ======================
# 2. Runtime Stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FitnessTracker.Api.dll"]

# Stage 1 — Build API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ONLY the API project, NOT the whole repo
COPY src/FitnessTracker.Api/FitnessTracker.Api.csproj ./FitnessTracker.Api/
RUN dotnet restore ./FitnessTracker.Api/FitnessTracker.Api.csproj

# Copy the entire API project now that restore is done
COPY src/FitnessTracker.Api ./FitnessTracker.Api

WORKDIR /src/FitnessTracker.Api
RUN dotnet publish -c Release -o /app/publish


# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FitnessTracker.Api.dll"]

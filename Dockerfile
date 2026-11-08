# syntax=docker/dockerfile:1
# RaceDay MVC web application (ASP.NET Core 8) - multi-stage build.
# Build context = repository root:  docker build -t raceday-mvc .

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file first so the restore layer is cached until dependencies change.
COPY RaceDay.Mvc/RaceDay.Mvc.csproj RaceDay.Mvc/
RUN dotnet restore RaceDay.Mvc/RaceDay.Mvc.csproj

COPY RaceDay.Mvc/ RaceDay.Mvc/
RUN dotnet publish RaceDay.Mvc/RaceDay.Mvc.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ApiSettings__BaseUrl=http://host.docker.internal:5100

COPY --from=build /app/publish .

# Run as the non-root "app" user that ships with the .NET 8 images.
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]

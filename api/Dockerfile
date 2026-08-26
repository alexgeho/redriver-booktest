# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
# Render/PaaS inject PORT; Program.cs binds to it. Fallback for local docker run.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "api.dll"]

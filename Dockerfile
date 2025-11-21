# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["DealEvaluator.sln", "./"]
COPY ["DealEvaluator.Web/DealEvaluator.Web.csproj", "DealEvaluator.Web/"]
COPY ["DealEvaluator.Application/DealEvaluator.Application.csproj", "DealEvaluator.Application/"]
COPY ["DealEvaluator.Domain/DealEvaluator.Domain.csproj", "DealEvaluator.Domain/"]
COPY ["DealEvaluator.Infrastructure/DealEvaluator.Infrastructure.csproj", "DealEvaluator.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "DealEvaluator.Web/DealEvaluator.Web.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/DealEvaluator.Web"
RUN dotnet publish "DealEvaluator.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "DealEvaluator.Web.dll"]
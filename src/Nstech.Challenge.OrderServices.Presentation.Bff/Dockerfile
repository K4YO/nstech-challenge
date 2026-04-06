# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Nstech.Challenge.OrderServices.AppCore.Domain/Nstech.Challenge.OrderServices.AppCore.Domain.csproj", "src/Nstech.Challenge.OrderServices.AppCore.Domain/"]
COPY ["src/Nstech.Challenge.OrderServices.AppCore.UseCases/Nstech.Challenge.OrderServices.AppCore.UseCases.csproj", "src/Nstech.Challenge.OrderServices.AppCore.UseCases/"]
COPY ["src/Nstech.Challenge.OrderServices.Infrastructure/Nstech.Challenge.OrderServices.Infrastructure.csproj", "src/Nstech.Challenge.OrderServices.Infrastructure/"]
COPY ["src/Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL/Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL.csproj", "src/Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL/"]
COPY ["src/Nstech.Challenge.OrderServices.Presentation.Bff/Nstech.Challenge.OrderServices.Presentation.Bff.csproj", "src/Nstech.Challenge.OrderServices.Presentation.Bff/"]

# Restore dependencies
RUN dotnet restore "src/Nstech.Challenge.OrderServices.Presentation.Bff/Nstech.Challenge.OrderServices.Presentation.Bff.csproj"

# Copy source code
COPY . .

# Build project
RUN dotnet build "src/Nstech.Challenge.OrderServices.Presentation.Bff/Nstech.Challenge.OrderServices.Presentation.Bff.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/Nstech.Challenge.OrderServices.Presentation.Bff/Nstech.Challenge.OrderServices.Presentation.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Nstech.Challenge.OrderServices.Presentation.Bff.dll"]


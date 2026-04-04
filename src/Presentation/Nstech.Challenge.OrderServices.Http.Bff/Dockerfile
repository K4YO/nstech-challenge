# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/AppCore/Nstech.Challenge.OrderServices.AppCore.Domain/Nstech.Challenge.OrderServices.AppCore.Domain.csproj", "src/AppCore/Nstech.Challenge.OrderServices.AppCore.Domain/"]
COPY ["src/AppCore/Nstech.Challenge.OrderServices.AppCore.UseCases/Nstech.Challenge.OrderServices.AppCore.UseCases.csproj", "src/AppCore/Nstech.Challenge.OrderServices.AppCore.UseCases/"]
COPY ["src/Infrastructure/Nstech.Challenge.OrderServices.Infrastructure/Nstech.Challenge.OrderServices.Infrastructure.csproj", "src/Infrastructure/Nstech.Challenge.OrderServices.Infrastructure/"]
COPY ["src/Presentation/Nstech.Challenge.OrderServices.Http.Bff/Nstech.Challenge.OrderServices.Http.Bff.csproj", "src/Presentation/Nstech.Challenge.OrderServices.Http.Bff/"]

# Restore dependencies
RUN dotnet restore "src/Presentation/Nstech.Challenge.OrderServices.Http.Bff/Nstech.Challenge.OrderServices.Http.Bff.csproj"

# Copy source code
COPY . .

# Build project
RUN dotnet build "src/Presentation/Nstech.Challenge.OrderServices.Http.Bff/Nstech.Challenge.OrderServices.Http.Bff.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/Presentation/Nstech.Challenge.OrderServices.Http.Bff/Nstech.Challenge.OrderServices.Http.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Nstech.Challenge.OrderServices.Http.Bff.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Presentation/VirtualWallet.Api/VirtualWallet.Api.csproj", "src/Presentation/VirtualWallet.Api/"]
COPY ["src/Core/VirtualWallet.Application/VirtualWallet.Application.csproj", "src/Core/VirtualWallet.Application/"]
COPY ["src/Core/VirtualWallet.Domain/VirtualWallet.Domain.csproj", "src/Core/VirtualWallet.Domain/"]
COPY ["src/Infrastructure/VirtualWallet.Infrastructure/VirtualWallet.Infrastructure.csproj", "src/Infrastructure/VirtualWallet.Infrastructure/"]

RUN dotnet restore "./src/Presentation/VirtualWallet.Api/VirtualWallet.Api.csproj"

COPY . .
WORKDIR "/src/src/Presentation/VirtualWallet.Api"
RUN dotnet build "./VirtualWallet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VirtualWallet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualWallet.Api.dll"]
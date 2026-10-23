FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AtlasBank.API/AtlasBank.API.csproj", "src/AtlasBank.API/"]
COPY ["src/AtlasBank.SharedKernel/AtlasBank.SharedKernel.csproj", "src/AtlasBank.SharedKernel/"]
COPY ["src/Modules/Accounts/AtlasBank.Accounts.Domain/AtlasBank.Accounts.Domain.csproj", "src/Modules/Accounts/AtlasBank.Accounts.Domain/"]
COPY ["src/Modules/Accounts/AtlasBank.Accounts.Application/AtlasBank.Accounts.Application.csproj", "src/Modules/Accounts/AtlasBank.Accounts.Application/"]
COPY ["src/Modules/Accounts/AtlasBank.Accounts.Infrastructure/AtlasBank.Accounts.Infrastructure.csproj", "src/Modules/Accounts/AtlasBank.Accounts.Infrastructure/"]
COPY ["src/Modules/Wallets/AtlasBank.Wallets.Domain/AtlasBank.Wallets.Domain.csproj", "src/Modules/Wallets/AtlasBank.Wallets.Domain/"]
COPY ["src/Modules/Wallets/AtlasBank.Wallets.Application/AtlasBank.Wallets.Application.csproj", "src/Modules/Wallets/AtlasBank.Wallets.Application/"]
COPY ["src/Modules/Wallets/AtlasBank.Wallets.Infrastructure/AtlasBank.Wallets.Infrastructure.csproj", "src/Modules/Wallets/AtlasBank.Wallets.Infrastructure/"]
COPY ["src/Modules/Payments/AtlasBank.Payments.Domain/AtlasBank.Payments.Domain.csproj", "src/Modules/Payments/AtlasBank.Payments.Domain/"]
COPY ["src/Modules/Payments/AtlasBank.Payments.Application/AtlasBank.Payments.Application.csproj", "src/Modules/Payments/AtlasBank.Payments.Application/"]
COPY ["src/Modules/Payments/AtlasBank.Payments.Infrastructure/AtlasBank.Payments.Infrastructure.csproj", "src/Modules/Payments/AtlasBank.Payments.Infrastructure/"]

RUN dotnet restore "src/AtlasBank.API/AtlasBank.API.csproj"

COPY . .
WORKDIR "/src/src/AtlasBank.API"
RUN dotnet build "AtlasBank.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AtlasBank.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AtlasBank.API.dll"]
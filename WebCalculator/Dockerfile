FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app
COPY WebCalculator.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish "WebCalculator/WebCalculator.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCalculator.dll"]

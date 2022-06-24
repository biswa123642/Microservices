FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY WebCalculator.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish "WebCalculator/WebCalculator.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/asp.net:5.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCalculator.dll"]

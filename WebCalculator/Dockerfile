FROM mcr.microsoft.com/dotnet/sdk:4.8 AS build
WORKDIR /app
COPY WebCalculator/WebCalculator.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish "WebCalculator/WebCalculator.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/asp.net:4.8
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCalculator/WebCalculator.dll"]

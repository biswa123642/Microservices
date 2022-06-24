FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app
COPY WebCalculator/WebCalculator.csproj .
RUN dotnet restore "WebCalculator.csproj"
COPY . .
RUN dotnet publish "WebCalculator/WebCalculator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCalculator.dll"]

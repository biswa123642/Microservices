FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/framework/sdk:6.0 AS build
WORKDIR /src
COPY ["WebCalculator.csproj", "."]
RUN dotnet restore "WebCalculator.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "WebCalculator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebCalculator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCalculator.dll"]

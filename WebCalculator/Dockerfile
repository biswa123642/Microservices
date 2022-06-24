FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8
ARG source
WORKDIR /inetpub/wwwroot
COPY WebCalculator/WebCalculator.csproj .
ENTRYPOINT ["dotnet", "WebCalculator.dll"]

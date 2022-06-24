FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8
ARG source
WORKDIR /inetpub/wwwroot
EXPOSE 80
EXPOSE 443
COPY WebCalculator/WebCalculator.csproj .
ENTRYPOINT ["dotnet", "WebCalculator.dll"]


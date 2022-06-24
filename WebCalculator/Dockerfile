FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8
ARG source
WORKDIR /inetpub/wwwroot
COPY WebCalculator/${source:-obj/Docker/publish} .


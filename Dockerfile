FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app
COPY WebCalculator.sln .
COPY WebCalculator/*.csproj ./DockerSample/
RUN nuget restore
COPY DockerSample/. ./DockerSample/
RUN msbuild /p:Configuration=Release -r:False
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019
WORKDIR /inetpub/wwwroot
COPY --from=build /app/DockerSample/. ./

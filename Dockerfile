FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 as build
WORKDIR "/src"

# Copy packages to your image and restore them
COPY WebApplication4.sln .
COPY WebApplication4/WebApplication4.csproj WebApplication4/WebApplication4.csproj
COPY WebApplication4/packages.config WebApplication4/packages.config
RUN nuget restore WebApplication4/packages.config -PackagesDirectory WebApplication4/packages

# Add files from source to the current directory and publish the deployment files to the folder profile
COPY . .
WORKDIR /src/WebApplication4
RUN msbuild WebApplication4.csproj /p:Configuration=Release /m /p:DeployOnBuild=true /p:PublishProfile=FolderProfile

# Layer the production runtime image
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019 as deploy

# Add the publish files into the right directory
WORKDIR /inetpub/wwwroot
COPY --from=build /src/WebApplication4/build/Release/PublishOutput .

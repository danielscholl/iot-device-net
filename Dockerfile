FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /usr/src/app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY lib ./
COPY Program.cs ./
COPY log4net.config ./
RUN dotnet publish -o out

# Build runtime image
FROM microsoft/dotnet:2.2-runtime
WORKDIR /usr/src/app
COPY --from=build-env /usr/src/app/out .
RUN mkdir -p cert

ENTRYPOINT ["dotnet", "IotDevice.dll"]

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY *.sln .
COPY Shop.Inventory.Service/*.csproj ./Shop.Inventory.Service/
COPY Shop.Inventory.UnitTest/*.csproj ./Shop.Inventory.UnitTest/

ARG PAT

# Set environment variables
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS '{"endpointCredentials":[{"endpoint":"https://pkgs.dev.azure.com/josephville12/_packaging/Commons/nuget/v3/index.json","username":"josephville12","password":"'${PAT}'"}]}'

# Get and install the Artifact Credential provider
RUN wget -O - https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | bash
RUN dotnet restore --interactive -s https://pkgs.dev.azure.com/josephville12/_packaging/Commons/nuget/v3/index.json

# copy full solution over
COPY . .
RUN dotnet build "./Shop.Inventory.Service/Shop.Inventory.Service.csproj"
RUN dotnet build "./Shop.Inventory.UnitTest/Shop.Inventory.UnitTest.csproj"

FROM build AS testrunner
WORKDIR /app/Shop.Inventory.UnitTest
CMD ["dotnet", "test", "--logger:trx"]

# run the unit tests
FROM build AS test
WORKDIR /app/Shop.Inventory.UnitTest
RUN dotnet test --logger:trx

# publish the API
FROM build AS publish
WORKDIR /app/Shop.Inventory.Service/
RUN dotnet publish -c Release -o out

# run the api
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app

COPY --from=publish /app/Shop.Inventory.Service/out ./
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Shop.Inventory.Service.dll"]
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS restore
WORKDIR /source
COPY Legate.sln /source/Legate.sln
COPY src/Legate/Legate.csproj /source/src/Legate/Legate.csproj
COPY tests/Legate.Tests/Legate.Tests.csproj /source/tests/Legate.Tests/Legate.Tests.csproj
COPY vendor/consuldotnet/Consul/Consul.csproj /source/vendor/consuldotnet/Consul/Consul.csproj
RUN set -xe \
    && dotnet restore

FROM restore AS build
ARG BUILD_VERSION=0.0.1
COPY . /source/
RUN set -xe \
    && dotnet build -c release -p:Version=${BUILD_VERSION}

FROM build AS test
ARG BUILD_VERSION=0.0.1
RUN set -xe \
    && dotnet test -c release -p:Version=${BUILD_VERSION}

FROM build AS publish
ARG BUILD_VERSION=0.0.1
RUN set -xe \
    && dotnet publish /source/src/Legate/Legate.csproj -c release -o /app -p:Version=${BUILD_VERSION}

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=publish /app /app
ENV ASPNETCORE_URLS=http://localhost:25010
ENTRYPOINT ["dotnet", "Legate.dll"]
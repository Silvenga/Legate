FROM mcr.microsoft.com/dotnet/sdk:5.0 AS restore
WORKDIR /source
COPY Legate.sln /source/Legate.sln
COPY src/Legate/Legate.csproj /source/src/Legate/Legate.csproj
COPY tests/Legate.Tests/Legate.Tests.csproj /source/tests/Legate.Tests/Legate.Tests.csproj
RUN set -xe \
    && dotnet restore

FROM restore AS build
COPY . /source/
RUN set -xe \
    && dotnet build -c release

FROM build AS test
RUN set -xe \
    && dotnet test -c release

FROM build AS publish
RUN set -xe \
    && dotnet publish /source/src/Legate/Legate.csproj -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=publish /app /app
ENTRYPOINT ["dotnet", "Legate.dll"]
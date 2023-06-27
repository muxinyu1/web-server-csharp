﻿FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app

COPY . /app

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0

WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "WebServer.dll"]



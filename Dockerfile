FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY . /app

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0

RUN apt-get update && \
    apt-get install -y python3 python3-pip && \
    pip3 install -U pip setuptools \

WORKDIR /app

COPY requirements.txt .

RUN pip3 install -r requirements.txt

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "WebServer.dll"]




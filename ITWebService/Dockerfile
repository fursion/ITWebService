﻿#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y procps
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ITWebService/ITWebService.csproj", "ITWebService/"]
RUN dotnet restore "ITWebService/ITWebService.csproj"
COPY . .
WORKDIR "/src/ITWebService"
RUN dotnet build "ITWebService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ITWebService.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
#
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/DutyInfo /var/ITWebService/DutyInfo
ENV TZ=Asia/Shanghai \
    DEBIAN_FRONTEND=noninteractive

RUN ln -fs /usr/share/zoneinfo/${TZ} /etc/localtime \
    && echo ${TZ} > /etc/timezone \
    && dpkg-reconfigure --frontend noninteractive tzdata \
    && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "ITWebService.dll"]

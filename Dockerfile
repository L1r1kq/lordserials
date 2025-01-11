{\rtf1\ansi\ansicpg1251\cocoartf2818
\cocoatextscaling0\cocoaplatform0{\fonttbl\f0\fswiss\fcharset0 Helvetica;\f1\fmodern\fcharset0 Courier;}
{\colortbl;\red255\green255\blue255;\red174\green176\blue183;\red23\green23\blue26;}
{\*\expandedcolortbl;;\csgenericrgb\c68235\c69020\c71765;\csgenericrgb\c9020\c9020\c10196;}
\paperw11900\paperh16840\margl1440\margr1440\vieww29200\viewh15760\viewkind0
\pard\tx720\tx1440\tx2160\tx2880\tx3600\tx4320\tx5040\tx5760\tx6480\tx7200\tx7920\tx8640\pardirnatural\partightenfactor0

\f0\fs24 \cf0 FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base\
WORKDIR /app\
\
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build\
WORKDIR /src\
COPY ["MyORMLibrary
\f1 \cf2 \cb3 \

\f0 \cf0 \cb1 /MyORMLibrary.csproj", \'abMyORMLibrary/\'ab]\
RUN dotnet restore "MyORMLibrary/MyORMLibrary.csproj"\
COPY . .\
WORKDIR "/src/MyORMLibrary"\
RUN dotnet build "MyORMLibrary.csproj" -c Release -o /app/build\
\
FROM build AS publish\
RUN dotnet publish "MyORMLibrary.csproj" -c Release -o /app/publish /p:UseAppHost=false\
\
FROM base AS final\
WORKDIR /app\
COPY --from=publish /app/publish .\
ENTRYPOINT ["dotnet", "MyORMLibrary.dll"]}

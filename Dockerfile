FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyORMLibrary
/MyORMLibrary.csproj", «MyORMLibrary/«]
RUN dotnet restore "MyORMLibrary/MyORMLibrary.csproj"
COPY . .
WORKDIR "/src/MyORMLibrary"
RUN dotnet build "MyORMLibrary.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyORMLibrary.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyORMLibrary.dll"]

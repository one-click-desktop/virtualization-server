FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["libs/*", "/usr/lib/"]
COPY . .
RUN dotnet restore 
RUN dotnet build --no-restore -c Release -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore -f net5.0 -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["assets/entry_point.sh", "entry_point.sh"]
RUN "ls"

ENTRYPOINT ["dotnet", "OneClickDesktop.VirtualizationServer.dll"]

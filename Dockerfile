FROM one-click-desktop/virtualization-server-runtime AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build --no-restore -f net5.0 -o /app/build VirtualizationServer/VirtualizationServer.csproj

FROM build AS publish
RUN dotnet publish --no-restore -f net5.0 -o /app/publish VirtualizationServer/VirtualizationServer.csproj

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
COPY ["assets/entry_point.sh", "entry_point.sh"]
RUN curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh -o /wait-for-it.sh && chmod +x /wait-for-it.sh
ENV CONFIG=config/docker-test

CMD ["/bin/bash", "entry_point.sh"]
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore 
RUN dotnet build --no-restore -f net5.0 -c Release -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore -f net5.0 -c Release -o /app/publish

FROM base AS final

#Install libvirt libraries
RUN apt-get update && apt-get install libvirt-clients vagrant -y
RUN vagrant plugin install vagrant-libvirt

WORKDIR /app
COPY --from=publish /app/publish .
COPY ["assets/entry_point.sh", "entry_point.sh"]

ENTRYPOINT ["/bin/bash", "entry_point.sh"]
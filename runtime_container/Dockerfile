FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base

RUN apt-get update && apt-get install libvirt-clients vagrant build-essential ruby-dev -y
RUN apt-get purge vagrant-libvirt -y
RUN vagrant plugin install vagrant-libvirt

ENTRYPOINT []
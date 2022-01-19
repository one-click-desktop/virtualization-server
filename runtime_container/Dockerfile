FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base

RUN apt-get update && apt-get install libvirt-dev build-essential ruby-dev ansible curl -y
RUN curl https://releases.hashicorp.com/vagrant/2.2.19/vagrant_2.2.19_x86_64.deb -o vagrant_2.2.19_x86_64.deb && dpkg -i vagrant_2.2.19_x86_64.deb
RUN vagrant plugin install vagrant-libvirt

ENTRYPOINT []
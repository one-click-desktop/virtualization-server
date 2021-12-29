#! /bin/bash

rm -rf libs
mkdir libs
cp /usr/lib/libvirt.so.0 libs/
cp /usr/lib/libvirt-qemu.so.0 libs/

docker build . -t one-click-desktop/virtualization-server
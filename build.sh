#! /bin/bash

cd runtime_container
./build.sh
cd ..
docker build . -t one-click-desktop/virtualization-server

# On running remember about:
# 1. Attaching host network
# 2. Attaching volume /var/run/libvirt/libvirt_sock:/var/run/libvirt/libvirt_sock
# 3. Attaching volume /var/lib/libvirt/:/var/lib/libvirt/
# 4. Attaching /root/.vagrant.d/boxes/ from container to local machine executive user's ~/.vagrant.d/boxes/
# 5. Attaching ./VirtualizationServer/config to /app
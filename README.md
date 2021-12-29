#Virtualization server

## Run inside container

Rememeber to pass unix socket from host libvirt daemon
```
docker run --name virtsrv-test -d -v /var/run/libvirt/libvirt-sock:/var/run/libvirt/libvirt-sock one-click-desktop/virtualization-server
```
# OneClickDesktop Virtualization Server

Virtualization Server module for OneClickDesktop. Responsible for hosting virtual machines and monitoring user connection status.

> ⚠️ Running more than one instance of application per host is highly discouraged. Vagrant can handle only one call at a time. Subsequent calls will return errors and can cause Vagrant to crash.

## Requirements

- [.NET 5](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)
- [libvirt](https://libvirt.org/) - dynamic library and daemon running on system
- [vagrant](https://www.vagrantup.com)
- [ansible](https://www.ansible.com/)

## Dependecies

Projects depends on modified [IDNT libvirt library](https://github.com/IDNT/AppBasics-Virtualization-Libvirt).
So running application require dynamic library `libvirt.so` installed and running `libvirtd` daemon.

## Building

Application is created for .NET5.0.
All references are defined in .csproj files.
With dotnet5.0-sdk installed building application should be as easy as:
```
dotnet build VirtualizationServer.sln
```
But forked IDNT library should build for older frameworks and can be problematic to build.

## Creating Vagrant Box compatible with OneClickDesktop

To run virtualization Server some Vagrant Box name should be passed.
With system is supplied arch based libvirt box [smogork/archlinux-rdp](https://app.vagrantup.com/smogork/boxes/archlinux-rdp).
It can be used as a template machine to make custom modifications.

If you want to create custom box from scratch please read [basic requirements](https://www.vagrantup.com/docs/boxes/base) from hashicorp.
In addition OneClickDesktop requires from box to:
1. Have some desktop manager (XFCE is used in template box).
2. Have RDP server ([xrdp](http://xrdp.org/) is used in template box)
3. Get address from DHCP to every new connected network interface.
4. Run `qemu-guest-agent` on virtual machine startup.

To build vagrant box for libvirt provider please read `vagrant-libvirt` box creation [documentation](https://github.com/vagrant-libvirt/vagrant-libvirt#create-box).

## Passing PCI devices to system

OneClickDesktop system can pass single physical GPU to created virtual machine.
It is realized by [PCI passthrough](https://access.redhat.com/documentation/en-us/red_hat_enterprise_linux/5/html/virtualization/chap-virtualization-pci_passthrough) mechanism.
This mechanism is really hardware dependent and rely on [IOMMU technology](https://www.amd.com/system/files/TechDocs/48882_IOMMU.pdf).
On ArchWiki is great tutorial of [PCI passthrough via OVMF](https://wiki.archlinux.org/title/PCI_passthrough_via_OVMF) and some troubleshooting advises.

Virtualization Server expects PCI addresses of GPUs in isolated IOMMU group.
Those GPUs must have stub drivers loaded to properly initialize on virtual machine startup. 
Then on startup machine has GPU attached by addresses passed in configuration file.

System can be run without GPUs passed. Just configure Virtualization Server to have 0 GPUs attached.

## Configuration

Configuration is stored in `VirtualizationServer/config`.

`virtsrv.ini` specifies server configuration. Files named `*-template.ini` specify machine types configurations.

Unspecified settings use default values. Configuration files contain default value entries commented out, with default value assigned.

Configuration used is specified on start with flag `-c`. By default configuration is taken from `./config/` directory.

### OneClickDesktop

- `VirtualizationServerId`: System-wide unique id of instance. Default value is `virtsrv-test`.
- `OverseerCommunicationShutdownTimeout`: Shutdown timeout (in seconds) for communication with Overseer. If server won't receive any message from Overseer, it will go down after this time. Default value is `120` seconds.
- `LibvirtUri`: Connection string to libvirt deamon. Default value is `qemu://system`.
- `VagrantFilePath`: Path to parametrized Vagrantfile used for virtual machines management.
- `PostStartupPlaybook`: Path to playbook provisioning machine after startup. Default value is `res/poststartup_playbook.yml`.
- `VagrantboxUri`: URI to Vagrant Box used in system.
- `BridgeInterfaceName`: Name of bridge interface for virtual machines. Default is `br0`.
- `BridgedNetwork`: Address of bridged network (CIDR format).
- `InternalRabbitMQHostname`: Hostname of internal RabbitMQ broker used for communication with overseers. Default value is `localhost`.
- `InternalRabbitMQPort`: Port of internal RabbitMQ broker used for communication with overseers. Default value is `5672`.
- `ExternalRabbitMQHostname`: Hostname of external RabbitMQ broker used for client connection monitoring. Default value is `localhost`.
- `ExternalRabbitMQPort`: Port of external RabbitMQ broker used for client connection monitoring. Default value is `5673`.
- `ClientHeartbeatChecksForMissing`: Amount of failed checks after which client is marked as missing/disconnected. Default is `2`.
- `ClientHeartbeatChecksDelay`: Time (in milliseconds) between client connection state checks. Default is `10000`.

### ServerResources

- `Cpus`: Number of total CPU logical cores available for system. Default is `2`.
- `Memory`: Amount of RAM available for system (in MiB). Default is `2048`.
- `Storage`: Amount of storage available for system. (in GiB). Default is `100`.
- `MachineTypes`: Comma separated list of machine type templates. Server will look for their configuration files in config folder with names matching machine type names with suffix `_template.ini`. IMPORTANT! Template name must contains ONLY characters from regexp `[a-zA-Z0-9\-]`. Otherwise libvirt will report domain name error.
- `GPUsCount`: Number of GPUs available for system.

### ServerGPU

> Number of instances of this group must match `GPUsCount` in `ServerResources` and start from `1`.

- `AddressCount`: Number of addresses associated with this GPU.
- `Address_{index}`: PCI address in format `{domain:4}:{bus:2}:{slot:2}.{function:1}`. Number of instances of this setting must match value of `AddressCount` and start from `1`.

For eg.
```
[ServerResources]
Cpus=6
Memory=4096
Storage=200
GPUsCount = 1
#Names for machine types
MachineTypes=cpu,gpu

[ServerGPU.1]
AddressCount = 2
Address_1 = 0000:03:00.0
Address_2 = 0000:03:00.1
```

### Template

> Template configuration file should start with group declaration with name same as file name (without extension).

- `HumanReadableName`: Human readable machine type name. This name is shown to user. If not set template name will be used.
- `Cpus`: Number of logical cores required to run machine. Default is `2`.
- `Memory`: Amount of RAM required to run machine (in MiB). Default is `512`.
- `Storage`: Amount of storage required to run machine (in GiB). Default is `100`.
- `AttachGPU`: Specify if machine should get dedicated GPU. Default is `false`.

## Run inside container

You can use prepared Dockerfile to create container and run application in docker.

To run app in docker:

1. Run `build.sh` to create container.
2. Run `docker run one-click-desktop/virtualization-server`. You need to add parameters specified below (configuration ones are optional).

Remember that container won't run libvirt services! It has to be run on host system independently!

### Important parameters

- Pass required libvirt volumes. It let application from container communicate with libvirt daemon running on host machine.

  ```BASH
  -v /var/run/libvirt/libvirt-sock:/var/run/libvirt/libvirt-sock
  -v /var/lib/libvirt/:/var/lib/libvirt/
  ```

- Pass local user vagrant boxes directory. It will be used to store vagrant boxes between runs. Also updating boxes can be done from outside container.

  ```BASH
  -v ${HOME}/.vagrant.d/boxes/:/root/.vagrant.d/boxes/
  ```

- Pass directory with modified configurations. Absolute path is required.

  ```BASH
  -v {PATH_TO_CONFIGS}:/app/config
  ```

- Override environmentral variable `CONFIG` containing path to used configuration directory. Path can be relative to `/app`.

  ```BASH
  --env CONFIG={PATH_TO_CONFIGS_IN_CONTAINER}
  ```

Example command with all parameters using example config:

```DOCKER
docker run --name virtsrv-test -d -v /var/run/libvirt/libvirt-sock:/var/run/libvirt/libvirt-sock
  -v /var/lib/libvirt/:/var/lib/libvirt/ -v $PWD/VirtualizationServer/config:/app/config --env CONFIG=config/docker-test one-click-desktop/virtualization-server
```

# OneClickDesktop Virtualization Server

Virtualization Server module for OneClickDesktop. Responsible for hosting virtual machines and monitoring user connection status.

## Requirements

- [.NET 5](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)
<!-- add libvirt and vagrant, maybe section about vagrant config? -->

## Configuration

Configuration is stored in `VirtualizationServer/config`.

`virtsrv.ini` specifies server configuration. Files named `*-template.ini` specify machine types configurations.

Unspecified settings use default values. Configuration files contain default value entries commented out, with default value assigned.

Configuration used is specified on start with flag `-c`.

### OneClickDesktop

- `VirtualizationServerId`: System-wide unique id of instance. Default value is `virtsrv-test`.
- `OverseerCommunicationShutdownTimeout`: Shutdown timeout (in seconds) for communication with Overseer. If server won't receive any message from Overseer, it will go down after this time. Default value is `120` seconds.
- `LibvirtUri`: Connection string to libvirt deamon. Default value is `qemu://system`.
- `VagrantFilePath`: Path to parametrized Vagrantfile used for virtual machines management.
- `VagrantboxUri`: URI to Vagrantbox used in system.
- `BridgeInterfaceName`: Name of bridge interface for virtual machines. Default is `br0`.
- `BridgedNetwork`: Address of bridged network (CIDR format).
- `InternalRabbitMQHostname`: Hostname of internal RabbitMQ broker used for communication with overseers. Default value is `localhost`.
- `InternalRabbitMQPort`: Port of internal RabbitMQ broker used for communication with overseers. Default value is `5672`.
- `ExternalRabbitMQHostname`: Hostname of external RabbitMQ broker used for client connection monitoring. Default value is `localhost`.
- `ExternalRabbitMQPort`: Port of external RabbitMQ broker used for client connection monitoring. Default value is `5673`.
- `ClientHeartbeatChecksForMissing`: Amount of failed checks after which client is marked as missing/disconnected. Default is `2`.
- `ClientHeartbeatChecksDelay`: Time (in miliseconds) between client connection state checks. Default is `10000`.

### ServerResources

- `Cpus`: Number of total CPU logical cores available for system. Default is `2`.
- `Memory`: Amount of RAM available for system (in MiB). Default is `2048`.
- `Storage`: Amount of storage available for system. (in GiB). Default is `100`.
- `MachineTypes`: Comma separated list of machine type templates. Server will look for their configuration files in config folder with names matching machine type names with prefix `-template.ini`.
- `GPUsCount`: Number of GPUs available for system.

### ServerGPU

> Number of instances of this group must match `GPUsCount` in `ServerResources` and start from `1`.

- `AddressCount`: Number of addresses associated with this GPU.
- `Address_{index}`: PCI address in format `{domain:4}:{bus:2}:{slot:2}.{function:1}`. Number of instances of this setting must match value of `AddressCount` and start from `1`.

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

### Important parameters

- Pass required libvirt volumes.

  ```BASH
  -v /var/run/libvirt/libvirt-sock:/var/run/libvirt/libvirt-sock
  -v /var/lib/libvirt/:/var/lib/libvirt/
  ```

- Pass local user vagrant boxes directory.

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

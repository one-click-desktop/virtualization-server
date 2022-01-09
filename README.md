# OneClickDesktop Virtualization Server

Virtualization Server module for OneClickDesktop. Responsible for hosting virtual machines and monitoring user connection status.

## Requirements

- [.NET 5](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)
<!-- add libvirt and vagrant, maybe section about vagrant config? -->

## Configuration

Configuration is stored in `VirtualizationServer/config`. `virtsrv.ini` specifies server configuration. Unspecified settings use default values.

### General

- `AllowedHosts`: Which hostnames the app should bind on. Value is semicolon-delimited list of host names without ports.
- `urls`: What addresses the app should listen on. Value is semicolon-delimited list of valid HTTP(S) URLs.

### Certificates

- `Path`: Path to TSL/SSL certificate in `.pfx` format.
- `Password`: Certificate password, if needed.
  > If address with `https` protocol is specified in `urls` then application won't start without certificate.

### Application

- `OverseerId`: System-wide unique id of instance. Default value is `overseer-test`.
- `RabbitMQHostname`: Hostname of RabbitMQ message broker for internal communication. Default value is `localhost`.
- `RabbitMQPort`: Port of RabbitMQ message broker for internal communication. Default value is `5672`.
- `ModelUpdateInterval`: Amount of time in seconds between model requests to virtualization servers. Defaul;ts is `60` seconds.
- `DomainShutdownTimeout`: Timeout in minutes for machine shutdown after client disconnected (or lost connection). Default is `15` minutes.
- `DomainShutdownCounterInterval`: Counter interval (in seconds) for domain shutdown checking. Should be divider of `DomainShutdownTimeout`. Default is `30` seconds.

## Run inside container

You can use prepared Dockerfile to create container and run application in docker.

If you want to enable HTTPS you need to generate valid certificate in `.pfx` format and set path to it in config file (path inside container). There is example certificate `overseer.pfx` provided in repository.

To run app in docker:

1. Run `build.sh` to create container.
2. Run `docker run one-click-desktop/overseer`. You may want to add some or all of parameters specified below.

### Important parameters

- Set port forwarding (docker ports depend on used configuration).

  ```BASH
  -p 80:5000 [-p 443:5001]
  ```

- Pass folder with modified configurations. Absolute path is required.

  ```BASH
  -v {PATH_TO_CONFIGS}:/overseer/config
  ```

- Pass certificate file. Absolute path is required. `PATH_TO_CERT_IN_CONTAINER` must match path specified in used configuration file.

  ```BASH
  -v {PATH_TO_CERT}:{PATH_TO_CERT_IN_CONTAINER}
  ```

Example command with all parameters using example certificate and config:

```DOCKER
docker run --name overseer -d -p 80:5000 -p 443:5001 -v $PWD/Overseer/config:/overseer/config -v $PWD/overseer.pfx:/overseer/overseer.pfx one-click-desktop/overseer
```

Rememeber to pass unix socket from host libvirt daemon

```
docker run --name virtsrv-test -d -v /var/run/libvirt/libvirt-sock:/var/run/libvirt/libvirt-sock one-click-desktop/virtualization-server
```

using System.Configuration;

namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// Configuration for OneClickDesktop system from Virtualization Server perspective
    /// </summary>
    public class VirtSrvConfiguration
    {
        /// <summary>
        /// System-wide unique virtualization server identifier
        /// </summary>
        public string VirtualizationServerId { get; set; } = "virtsrv-test";
        /// <summary>
        /// Shutdown timeout (in seconds) for overseer communication.
        /// After this time server wil go down if overseer won't send any message.
        /// </summary>
        public int OversserCommunicationShutdownTimeout { get; set; } = 120;
        /// <summary>
        /// Connection string to libvirt daemon
        /// </summary>
        public string LibvirtUri { get; set; } = "qemu:///system";
        /// <summary>
        /// Path to parametrized vagrantfile using to virtual machines management
        /// </summary>
        public string VagrantFilePath { get; set; } = null;
        /// <summary>
        /// Uri for using Vagrantbox in system
        /// </summary>
        public string VagrantboxUri { get; set; } = null;
        /// <summary>
        /// Path on host machine to UEFI image used to build vagrantbox.
        /// </summary>
        public string UefiPath { get; set; } = null;
        /// <summary>
        /// Path on host machine to NVRAM image used on box building.
        /// It should be hardlinked before attaching to machine and unlink after destroying.
        /// </summary>
        public string NvramPath { get; set; } = null;
        /// <summary>
        /// Bridge interface name for virtual machines
        /// </summary>
        public string BridgeInterfaceName { get; set; } = "br0";
        /// <summary>
        /// Bridged network address (CIDR format)
        /// </summary>
        public string BridgedNetwork { get; set; } = null;
        /// <summary>
        /// Hostname of internal RabbitMQ broker (overseers communication)
        /// </summary>
        public string InternalRabbitMQHostname {get; set;} = "localhost";
        /// <summary>
        /// Port of internal RabbitMQ broker (overseers communication)
        /// </summary>
        public int InternalRabbitMQPort {get; set;} = 5672;
        /// <summary>
        /// Hostname of external RabbitMQ broker (client heartbeat communication)
        /// </summary>
        public string ExternalRabbitMQHostname {get; set;} = "localhost";
        /// <summary>
        /// Port of external RabbitMQ broker (client heartbeat communication)
        /// </summary>
        public int ExternalRabbitMQPort {get; set;} = 5673;
        /// <summary>
        /// Amount of failed checks after which client is marked as missing
        /// </summary>
        public int ClientHeartbeatChecksForMissing { get; set; } = 2;
        /// <summary>
        /// Time between heartbeat checks (in ms)
        /// </summary>
        public int ClientHeartbeatChecksDelay { get; set; } = 10000;

        /// <summary>
        /// Path to playbook provisioning machine after startup.
        /// </summary>
        public string PostStartupPlaybook { get; set; } = "";
    }
}
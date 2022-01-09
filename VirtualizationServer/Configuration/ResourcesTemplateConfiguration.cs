namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// Configuration class describing required resources by machine of some type
    /// </summary>
    public class ResourcesTemplateConfiguration
    {
        /// <summary>
        /// Human readable machine type name. If not set template name will be used.
        /// </summary>
        public string HumanReadableName { get; set; } = null;
        /// <summary>
        /// Number of logical cores required to run machine
        /// </summary>
        public int Cpus { get; set; } = 2;
        /// <summary>
        /// Amount of memory required to run machine (in MiB)
        /// </summary>
        public int Memory { get; set; } = 512;
        /// <summary>
        /// Amount of Storage required to run machine (in GiB)
        /// </summary>
        public int Storage { get; set; }

        /// <summary>
        /// Wished model of GPU. Implisies that machine should has attached GPU on startup.
        /// </summary>
        public bool AttachGPU { get; set; } = false;

    }
}
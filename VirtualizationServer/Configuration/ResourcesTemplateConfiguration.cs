namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// Configuration class describing required resources by machine of some type
    /// </summary>
    public class ResourcesTemplateConfiguration
    {
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
        public string WishedGPU { get; set; } = null;

    }
}
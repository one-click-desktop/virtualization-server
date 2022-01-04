using CommandLine;

namespace OneClickDesktop.VirtualizationServer
{
    public class Options
    {
        [Option('c', "config", Default = "config", Required = false,
            HelpText = "Path to folder with configuration files. Inside server will be searching for virtsrv.ini config file.")]
        public string ConfigurationFolderPath { get; set; }
    }
}
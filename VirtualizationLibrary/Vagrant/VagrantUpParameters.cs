using System.Text;
using System.Text.Json;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    public class VagrantUpParameters
    {
        public string BoxName { get; set; }
        public string Hostname { get; set; }
        public int Memory { get; set; }
        public int CpuCores { get; set; }

        public VagrantUpParameters(string name, string hostname, int memory, int cpus)
        {
            BoxName = name;
            Hostname = hostname;
            Memory = memory;
            CpuCores = cpus;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize<VagrantUpParameters>(this);
        }

        public string FormatForExecute()
        {
            StringBuilder str = new StringBuilder();

            str.Append($"--vm-name={BoxName}");str.Append(" ");
            str.Append($"--cpus={CpuCores}");str.Append(" ");
            str.Append($"--memory={Memory}");str.Append(" ");
            str.Append($"--hostname={Hostname}");
            
            return str.ToString();
        }
    }
}
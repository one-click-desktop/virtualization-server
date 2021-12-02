using System.Text;
using System.Text.Json;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    public class VagrantParameters
    {
        public string BoxName { get; set; }
        public string Hostname { get; set; }
        public int Memory { get; set; }
        public int CpuCores { get; set; }
        public string VagrantBox { get; set; }

        public VagrantParameters(string vagrantBox, string name, string hostname, int memory, int cpus)
        {
            VagrantBox = vagrantBox;
            BoxName = name;
            Hostname = hostname;
            Memory = memory;
            CpuCores = cpus;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize<VagrantParameters>(this);
        }

        private void AppendParameter(StringBuilder str, string name, string val, bool TrailingSpace = true)
        {
            str.Append($"--{name}=\"{val}\"");
            if (TrailingSpace)
                str.Append(" ");
        }

        public string FormatForExecute()
        {
            StringBuilder str = new StringBuilder();
            
            AppendParameter(str, "boxname", VagrantBox);
            AppendParameter(str, "vm-name", BoxName);
            AppendParameter(str, "cpus", CpuCores.ToString());
            AppendParameter(str, "memory", Memory.ToString());
            AppendParameter(str, "hostname", Hostname, false);

            return str.ToString();
        }
    }
}
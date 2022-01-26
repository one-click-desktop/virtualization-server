using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using OneClickDesktop.BackendClasses.Model.Resources;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    /// <summary>
    /// Abstrakcyjna reprezentacja parametru przekazywanego do Vagrantfile
    /// </summary>
    public abstract class AbstractParameter
    {
        protected const string ENV_PREFIX = "OCD_";
        protected const string CONSOLE_PREFIX = "--";

        /// <summary>
        /// Nazwa zmiennej srodowiskowej na ktorej powinna znalezc sie wartosc parametru
        /// </summary>
        public virtual string EnvironmentVariable => ENV_PREFIX + envSuffix;
        /// <summary>
        /// Sformatowany parametr wywolania wagranta do którego trzeba przypisac wartość parametru
        /// </summary>
        public virtual string ConsoleParameter => CONSOLE_PREFIX + cliSuffix;

        /// <summary>
        /// Wartośc parametru
        /// </summary>
        public string Value => value;

        protected string value;
        protected string envSuffix;
        protected string cliSuffix;

        public AbstractParameter(string value, string envSuffix, string cliSuffix)
        {
            this.value = value;
            this.envSuffix = envSuffix;
            this.cliSuffix = cliSuffix;
        }
        
        /// <summary>
        /// Ustawia zmienna środowiskowa z waroytścią parametru do Vagrantfile
        /// </summary>
        /// <param name="env">Słownik reprezentujący środowisko</param>
        public virtual void SetEnironmentalVariable(StringDictionary env)
        {
            env[EnvironmentVariable] = value;
        }
        
        /// <summary>
        /// Generuje odpowiednio sformatyowany parametr z wartoscia
        /// </summary>
        /// <returns>Gotowy parametr z wartością</returns>
        public virtual string FormatConsoleParameter()
        {
            return $"{ConsoleParameter}={value}";
        }
    }

    public class BoxNameParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "BOXNAME";
        public const string CLI_SUFFIX = "boxname";

        public BoxNameParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class NameParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "VMNAME";
        public const string CLI_SUFFIX = "vm-name";

        public NameParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class CpusParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "CPUS";
        public const string CLI_SUFFIX = "cpus";

        public CpusParameter(int value) : base(value.ToString(), ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class MemoryParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "MEMORY";
        public const string CLI_SUFFIX = "memory";

        public MemoryParameter(int value) : base(value.ToString(), ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class HostnameParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "HOSTNAME";
        public const string CLI_SUFFIX = "hostname";

        public HostnameParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class BridgeParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "BRIDGE";
        public const string CLI_SUFFIX = "bridge";

        public BridgeParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class GpuParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "GPU";
        public const string CLI_SUFFIX = "gpu";

        public GpuParameter(GpuId gpuAddresses) : base("", ENV_SUFFIX, CLI_SUFFIX)
        {
            StringBuilder val = new StringBuilder();

            val.Append(gpuAddresses.PciIdentifiers.First());
            foreach (PciAddressId addr in gpuAddresses.PciIdentifiers.Skip(1))
            {
                val.Append(",");
                val.Append(addr);
            }

            value = val.ToString(); 
        }
    }
    
    public class LibvirtUriParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LIBVIRT_URI";
        public const string CLI_SUFFIX = "libvirt-uri";

        public LibvirtUriParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class PoststartupPlaybookParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "POSTSTARTUP_PLAYBOOK";
        public const string CLI_SUFFIX = "poststartup-playbook";

        public PoststartupPlaybookParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class UefiParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "UEFI";
        public const string CLI_SUFFIX = "uefi";

        public UefiParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }
    
    public class NvramParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "NVRAM";
        public const string CLI_SUFFIX = "nvram";

        public NvramParameter(string value) : base(value, ENV_SUFFIX, CLI_SUFFIX)
        { }
    }

    /// <summary>
    /// Klasa opisująca zbiór parametrów dla Vagrantfile
    /// </summary>
    public class VagrantParameters
    {
        private List<AbstractParameter> parameters;
        
        public VagrantParameters()
        {
            parameters = new List<AbstractParameter>();
        }
        
        /// <summary>
        /// Constructor for destroy command
        /// </summary>
        /// <param name="boxName"></param>
        /// <param name="name"></param>
        /// <param name="hostname"></param>
        /// <param name="bridgeDevice"></param>
        public VagrantParameters(string boxName, string name, string hostname, string bridgeDevice, string libvirtUri, string nvramPath)
        {
            parameters = new List<AbstractParameter>()
            {
                new BoxNameParameter(boxName),
                new NameParameter(name),
                new HostnameParameter(hostname),
                new CpusParameter(0),
                new MemoryParameter(0),
                new BridgeParameter(bridgeDevice),
                new LibvirtUriParameter(libvirtUri),
                new PoststartupPlaybookParameter(""),
                new NvramParameter(nvramPath)
            };   
        }
        
        /// <summary>
        /// Constructor for create command
        /// </summary>
        /// <param name="boxName"></param>
        /// <param name="name"></param>
        /// <param name="hostname"></param>
        /// <param name="bridgeDevice"></param>
        /// <param name="memory"></param>
        /// <param name="cpus"></param>
        public VagrantParameters(string boxName, string name, string hostname, string bridgeDevice, int memory, int cpus, string poststartupPlaybook, string libvirtUri, string uefi, string nvram)
        {
            parameters = new List<AbstractParameter>()
            {
                new BoxNameParameter(boxName),
                new NameParameter(name),
                new HostnameParameter(hostname),
                new CpusParameter(cpus),
                new MemoryParameter(memory),
                new BridgeParameter(bridgeDevice),
                new LibvirtUriParameter(libvirtUri),
                new PoststartupPlaybookParameter(poststartupPlaybook),
                new UefiParameter(uefi),
                new NvramParameter(nvram)
            };   
        }
    
        /// <summary>
        /// Dodaj parametr do zbioru. Jeżeli istnieje już taki to zostanie usunięty przed dodaniem.
        /// </summary>
        /// <param name="parameter">Dodawany parametr</param>
        public void AddParameter(AbstractParameter parameter)
        {
            var existing = parameters.FirstOrDefault(p => p.GetType() == parameter.GetType());
            if (existing != null)
                parameters.Remove(existing);
            parameters.Add(parameter);
        }

        /// <summary>
        /// PObiera warotśc parametru podanego typu
        /// </summary>
        /// <param name="parameterType">Typ szukanego paramteru</param>
        /// <returns>Wartość parametru podanego typu. Pusty string w przypadku braku parametru w zbiorze.</returns>
        public string GetParameterValue(Type parameterType)
        {
            return parameters.FirstOrDefault(p => p.GetType() == parameterType)?.Value ?? "";
        }
        
        public override string ToString()
        {
            return JsonSerializer.Serialize<VagrantParameters>(this);
        }
        
        /// <summary>
        /// Formatuje parametry jako ciąg do podania w konsoli.
        /// </summary>
        /// <returns>Ciąg parametrów odpowienio sformatowany do wykonania</returns>
        public string FormatForExecute()
        {
            StringBuilder str = new StringBuilder();

            int counter = 0;
            foreach (AbstractParameter parameter in parameters)
            {
                str.Append(parameter.FormatConsoleParameter());
                counter++;
                if (counter < parameters.Count)
                    str.Append(" ");
            }

            return str.ToString();
        }

        /// <summary>
        /// Dla podanego środowiska definiowane sa zmienne z wartościami parametrów.
        /// </summary>
        /// <param name="env">Słownik reprezentujący środowisko</param>
        public void DefineEnvironmentalVariables(StringDictionary env)
        {
            foreach (AbstractParameter parameter in parameters)
                parameter.SetEnironmentalVariable(env);
        }
    }
}
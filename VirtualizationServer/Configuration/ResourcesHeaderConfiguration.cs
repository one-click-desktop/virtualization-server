using System.Collections.Generic;
using System.Linq;

namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// Class defines header of resources cofiguration.
    /// Data is parsing from ini file.
    /// </summary>
    public class ResourcesHeaderConfiguration
    {
        /// <summary>
        /// Numer of total cpu logical cores accesible by system
        /// </summary>
        public int Cpus { get; set; } = 2;
        /// <summary>
        /// Amount of memory accessible by system (in MiB).
        /// </summary>
        public int Memory { get; set; } = 2048;
        /// <summary>
        /// Amount of storaga accesible by system (in GiB)
        /// </summary>
        public int Storage { get; set; } = 100;
        
        /// <summary>
        /// Comma separated list of machine templates machines by server.
        /// </summary>
        public string MachineTypes { get; set; }

        /// <summary>
        /// Get list of machine templates based on <c>MachineTypes</c>.
        /// </summary>
        /// <returns>List of string names of templates</returns>
        public List<string> GetMachineTemplates()
        {
            return MachineTypes
                .Split(",")
                .Select(s => s.Trim())
                .ToList();
        }
    }
}
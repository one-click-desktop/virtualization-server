using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.BackendClasses.Model.Types;

namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// Class contains configuration of virtualization server resources.
    /// </summary>
    public class ResourcesConfiguration
    {
        private const string GPUSectionPrefix = "ServerGPU.";
        private const string GPUAddressPrefix = "Address_";
        private const string AddressCountKey = "AddressCount";

        private IConfigurationRoot baseConfig;    
        private ResourcesHeaderConfiguration header;
        private List<(string, ResourcesTemplateConfiguration)> templates = new List<(string, ResourcesTemplateConfiguration)>();
        private List<GpuId> attachedGpus = new List<GpuId>();

        public ResourcesConfiguration(IConfigurationRoot baseConfig, string basePath)
        {
            this.baseConfig = baseConfig;
            var resourcesHeaderSection = baseConfig.GetSection("ServerResources");
            this.header = resourcesHeaderSection.Get<ResourcesHeaderConfiguration>();

            ParseMachineTemplates(basePath);
            ParseServerGPUs(header.GPUsCount);
        }

        private void ParseMachineTemplates(string basePath)
        {
            IConfigurationBuilder configDraft = new ConfigurationBuilder()
                .SetBasePath(basePath);
            foreach (string template_name in header.GetMachineTemplates())
                configDraft = configDraft.AddIniFile($"{template_name}_template.ini");
            IConfigurationRoot config = configDraft.Build();

            foreach (string template_name in header.GetMachineTemplates())
            {
                var template_section = config.GetSection($"{template_name}_template");
                templates.Add((template_name, template_section.Get<ResourcesTemplateConfiguration>()));
            }
        }

        private T GetValueFromSection<T>(IConfigurationSection section, string key)
        {
            T val =  section.GetValue<T>(key);
            if (EqualityComparer<T>.Default.Equals(default(T), val))
                throw new ArgumentException($"Missing key {section.Key}:{key} in configuration file");
            return val;
        }
        
        private void ParseServerGPUs(int gpuCount)
        {
            for (int i = 1; i <= gpuCount; ++i)
            {
                string sectionName = GPUSectionPrefix + i.ToString();
                var section = baseConfig.GetSection(sectionName);

                if (section.GetChildren().Count() == 0)
                    throw new ArgumentException($"Missing section {sectionName} in configuration file.");

                int addressCount = GetValueFromSection<int?>(section, AddressCountKey) ?? -1;
                List<PciAddressId> parsedAddrs = new List<PciAddressId>();
                for (int addri = 1; addri <= addressCount; ++addri)
                {
                    string addr = GetValueFromSection<string>(section, GPUAddressPrefix + addri.ToString());
                    parsedAddrs.Add(PciAddressId.Parse(addr));
                }
                attachedGpus.Add(new GpuId(parsedAddrs));
            }
        }

        /// <summary>
        /// Get total server resources
        /// </summary>
        /// <returns>Server resources in model format</returns>
        public ServerResources GetServerResources()
        {
            return new ServerResources(header.Memory, header.Cpus, header.Storage, attachedGpus);
        }

        /// <summary>
        /// Get dictionary with named template resources
        /// </summary>
        /// <returns>Template resources</returns>
        public Dictionary<string, TemplateResources> GetTemplateResources()
        {
            return templates.ToDictionary(
                d => d.Item1,
                d =>
                    new TemplateResources(
                        new MachineType()
                        {
                            TechnicalName = d.Item1,
                            HumanReadableName = d.Item2.HumanReadableName
                        },
                        d.Item2.Memory,
                        d.Item2.Cpus,
                        d.Item2.Storage,
                        false)
                );
        }
    }
}
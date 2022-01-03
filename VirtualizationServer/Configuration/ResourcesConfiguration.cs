using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using OneClickDesktop.BackendClasses.Model.Resources;

namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// Class contains configuration of virtualization server resources.
    /// </summary>
    public class ResourcesConfiguration
    {
        private ResourcesHeaderConfiguration header;
        private List<(string, ResourcesTemplateConfiguration)> templates = new List<(string, ResourcesTemplateConfiguration)>();

        public ResourcesConfiguration(ResourcesHeaderConfiguration header,
            string basePath)
        {
            this.header = header;
            
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

        /// <summary>
        /// Get total server resources
        /// </summary>
        /// <returns>Server resources in model format</returns>
        public ServerResources GetServerResources()
        {
            return new ServerResources(header.Memory, header.Cpus, header.Storage, Array.Empty<GpuId>());
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
                    new TemplateResources(d.Item2.Memory, d.Item2.Cpus, d.Item2.Storage,
                        false));//TODO: GPUid nie ma sensu tutaj - potrzeba nazwy wlasnej gpu
        }
    }
}
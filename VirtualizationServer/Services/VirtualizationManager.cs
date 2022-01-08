using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.VirtualizationLibrary.Libvirt;
using OneClickDesktop.VirtualizationLibrary.Vagrant;
using OneClickDesktop.VirtualizationServer.Configuration;

namespace OneClickDesktop.VirtualizationServer.Services
{
    /// <summary>
    /// Klasa zarządza maszynami wirtualnymi działającymi pod pieczą systemu.
    /// </summary>
    public class VirtualizationManager: IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private object vagrantLock = new object();
        
        private VirtSrvConfiguration conf;
        private LibvirtWrapper libvirt;
        private VagrantWrapper vagrant;

        public VirtualizationManager(VirtSrvConfiguration conf)
        {
            logger.Info("Creating VirtualizationManager");
            this.conf = conf;
            libvirt = new LibvirtWrapper(conf.LibvirtUri);
            vagrant = new VagrantWrapper(conf.VagrantFilePath);
        }

        /// <summary>
        /// Creates machine of doesn't exists and starts it
        /// </summary>
        /// <remarks>
        /// Should update model when machine changes states(?)
        /// </remarks>
        /// <param name="domainName">Name of machine</param>
        /// <param name="resource">Resources for creating machine</param>
        /// <param name="address">Address of machine inside bridged network</param>
        /// <returns>
        /// true - operation succeeded
        /// false - operation failed
        /// </returns>
        public bool DomainStartup(string domainName, TemplateResources resource, out IPAddress address)
        {
            address = null;
            if (libvirt.DoesDomainActive(domainName))
                return false;

            try
            {
                VagrantParameters parameters = new VagrantParameters
                (
                    conf.VagrantboxUri,
                    domainName,
                    domainName,
                    conf.BridgeInterfaceName,
                    resource.Memory,
                    resource.CpuCores
                );
                lock (vagrantLock)
                {
                    vagrant.VagrantUp(parameters);
                }
                logger.Info("Vagrant up command finished");

                IPNetwork bridgedNetwork = IPNetwork.Parse(conf.BridgedNetwork);
                address = TryGetDomainAddress(domainName, bridgedNetwork);
                if (address == null)
                {
                    lock (vagrantLock)
                    {
                        logger.Warn(
                            $"Domain {domainName} doesn't have any address at network {bridgedNetwork}. Destroying.");
                        vagrant.BestEffortVagrantDestroy(parameters);
                    }

                    return false;
                }
                
                return true;
            }
            catch (VagrantException e)
            {
                logger.Error(e, "Vagrant up returned with error");
                return false;
            }
        }

        private IPAddress TryGetDomainAddress(string domainName, IPNetwork bridgedNetwork, int askCount = 5, int askIntervalMs = 500)
        {
            IPAddress result = null;
            int askCounter = 0;
            
            while (result == null && askCounter < askCount)
            {
                var addresses = libvirt.GetDomainsNetworkAddresses(domainName);
                if (addresses?.Any() ?? false)
                    result = addresses?.FirstOrDefault(bridgedNetwork.Contains);
            }

            return result;
        }

        /// <summary>
        /// Shutdowns machine
        /// </summary>
        /// <remarks>
        /// Should update model when machine changes states(?)
        /// </remarks>
        /// <param name="domainName">Name of machine</param>
        /// <returns>
        /// true - operation succeeded
        /// false - operation failed
        /// </returns>
        public bool DomainShutdown(string domainName)
        {
            if (!libvirt.DoesDomainExist(domainName))
                return false;
            
            try
            {
                VagrantParameters parameters = new VagrantParameters
                (
                    conf.VagrantboxUri,
                    domainName,
                    domainName,
                    conf.BridgeInterfaceName
                );
                lock (vagrantLock)
                {
                    vagrant.VagrantDestroy(parameters);
                }

                return true;
            }
            catch (VagrantException e)
            {
                logger.Error(e, "Vagrant destroy returned with error");
                return false;
            }
        }

        public void DomainCleanupOnShutdown(IEnumerable<string> domainNames)
        {
            foreach (string name in domainNames)
                DomainShutdown(name);
        }

        public void Dispose()
        {
            libvirt?.Dispose();
        }
    }
}
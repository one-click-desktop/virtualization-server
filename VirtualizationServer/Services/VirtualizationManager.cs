using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.VirtualizationLibrary.Ansible;
using OneClickDesktop.VirtualizationLibrary.Libvirt;
using OneClickDesktop.VirtualizationLibrary.Vagrant;
using OneClickDesktop.VirtualizationServer.Configuration;

namespace OneClickDesktop.VirtualizationServer.Services
{
    public static class ParametersFactory
    {
        public static AnsibleParameters AnsibleFromConfiguration(NfsConfiguration nfsConf,
            LdapConfiguration ldapConf)
        {
            return new AnsibleParameters(
                ldapConf.Uri,
                ldapConf.Domain,
                ldapConf.ReadOnlyDn,
                ldapConf.ReadOnlyPassword,
                ldapConf.AdminDn,
                ldapConf.GroupsDn,
                ldapConf.UsersDn,
                nfsConf.ServerName,
                nfsConf.HomePath
            );
        }

        public static VagrantParameters VagrantForMachine(string domainName, TemplateResources resource, string nvramPath, VirtSrvConfiguration conf)
        {
            return new VagrantParameters
            (
                conf.VagrantboxUri,
                domainName,
                domainName,
                conf.BridgeInterfaceName,
                resource.Memory,
                resource.CpuCores,
                conf.PostStartupPlaybook,
                conf.LibvirtUri,
                conf.UefiPath,
                nvramPath
            );
        }

        public static VagrantParameters VagrantForShutdown(string domainName,
            string nvramPath, VirtSrvConfiguration conf)
        {
            return new VagrantParameters
            (
                conf.VagrantboxUri,
                domainName,
                domainName,
                conf.BridgeInterfaceName,
                conf.LibvirtUri,
                nvramPath,
                conf.UefiPath
            );
        }
    }
    
    /// <summary>
    /// Klasa zarz??dza maszynami wirtualnymi dzia??aj??cymi pod piecz?? systemu.
    /// </summary>
    public class VirtualizationManager : IDisposable
    {
        private const string nvramPrefix = "/var/lib/libvirt/qemu/nvram";
        
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private object vagrantLock = new object();

        private VirtSrvConfiguration virtsrvConf;
        private LdapConfiguration ldapConf;
        private NfsConfiguration nfsConf;
        private LibvirtWrapper libvirt;
        private VagrantWrapper vagrant;

        public VirtualizationManager(VirtSrvConfiguration systemConfig, NfsConfiguration nfsConf, LdapConfiguration ldapConf)
        {
            logger.Info("Creating VirtualizationManager");
            this.virtsrvConf = systemConfig;
            this.nfsConf = nfsConf;
            this.ldapConf = ldapConf;
            libvirt = new LibvirtWrapper(virtsrvConf.LibvirtUri);
            vagrant = new VagrantWrapper(virtsrvConf.VagrantFilePath);
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
        public bool DomainStartup(string domainName, TemplateResources resource, GpuId attachedGPU,
            out IPAddress address)
        {
            address = null;
            if (libvirt.DoesDomainActive(domainName))
                return false;
            string nvramPath = Path.Combine(nvramPrefix, domainName + ".fd");

            try
            {
                AnsibleParameters aParams = ParametersFactory.AnsibleFromConfiguration(nfsConf, ldapConf);
                VagrantParameters vParams = ParametersFactory.VagrantForMachine(domainName, resource, nvramPath, virtsrvConf);
                if (attachedGPU != null && attachedGPU.PciIdentifiers.Count > 0)
                    vParams.AddParameter(new GpuParameter(attachedGPU));

                lock (vagrantLock)
                {
                    if (virtsrvConf.NvramPath?.Length > 0 && virtsrvConf.NvramPath?.Length > 0)
                        File.Copy(virtsrvConf.NvramPath, nvramPath, true);
                    vagrant.VagrantUp(vParams, aParams);
                }

                logger.Info("Vagrant up command finished");

                IPNetwork bridgedNetwork = IPNetwork.Parse(virtsrvConf.BridgedNetwork);
                address = TryGetDomainAddress(domainName, bridgedNetwork, 20);
                if (address == null)
                {
                    lock (vagrantLock)
                    {
                        logger.Warn(
                            $"Domain {domainName} doesn't have any address at network {bridgedNetwork}. Destroying.");
                        vagrant.BestEffortVagrantDestroy(vParams);
                        File.Delete(nvramPath);
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

        private IPAddress TryGetDomainAddress(string domainName, IPNetwork bridgedNetwork, int askCount = 10,
            int askIntervalMs = 500)
        {
            IPAddress result = null;
            int askCounter = 0;

            while (result == null && askCounter < askCount)
            {
                var addresses = libvirt.GetDomainsNetworkAddresses(domainName);
                if (addresses?.Any() ?? false)
                    result = addresses?.FirstOrDefault(bridgedNetwork.Contains);
                Thread.Sleep(askIntervalMs);
                askCounter++;
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

            string nvramPath = Path.Combine(nvramPrefix, domainName + ".fd");
            try
            {
                VagrantParameters vParams = ParametersFactory.VagrantForShutdown(domainName, nvramPath, virtsrvConf);
                lock (vagrantLock)
                {
                    vagrant.VagrantDestroy(vParams);
                    File.Delete(nvramPath);
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
using System;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.VirtualizationLibrary.Libvirt;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationServer.Services
{
    /// <summary>
    /// Klasa zarządza maszynami wirtualnymi działającymi pod pieczą systemu.
    /// </summary>
    public class VirtualizationManager: IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        private LibvirtWrapper libvirt;
        private VagrantWrapper vagrant;

        public VirtualizationManager(string libvirtUri, string vagrantFile)
        {
            logger.Info("Creating VirtualizationManager");
            libvirt = new LibvirtWrapper(libvirtUri);
            vagrant = new VagrantWrapper(vagrantFile);
        }

        /// <summary>
        /// Creates machine of doesn't exists and starts it
        /// </summary>
        /// <remarks>
        /// Should update model when machine changes states(?)
        /// </remarks>
        /// <param name="domainName">Name of machine</param>
        /// <param name="domainType">Type of machine</param>
        /// <returns>
        /// true - operation succeeded
        /// false - operation failed
        /// </returns>
        public bool DomainStartup(string domainName, MachineType domainType)
        {
            // TODO: add implementation
            return true;
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
            // TODO: add implementation
            return true;
        }

        public void Dispose()
        {
            libvirt?.Dispose();
        }
    }
}
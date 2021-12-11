using System;
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

        public void Dispose()
        {
            libvirt?.Dispose();
        }
    }
}
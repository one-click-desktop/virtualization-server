using System;
using IDNT.AppBasics.Virtualization.Libvirt;

namespace OneClickDesktop.VirtualizationLibrary.Libvirt
{
    /// <summary>
    /// Klasa zajmuje się komunikacją z libvirtem i udostepnia interfejs dla VirtualizationManagera.
    /// </summary>
    public class LibvirtWrapper: IDisposable
    {
        private LibvirtConnection connection;

        public LibvirtWrapper(string uri)
        {
            connection = LibvirtConnection.Create.WithLocalAuth().WithMetricsDisabled().Connect();
        }
        
        public LibvirtDomain GetDomainByName(string name) => connection.GetDomainByName(name);
        
        public bool DoesDomainExist(string name)
        {
            throw new NotImplementedException();
        }
        
        public bool DoesDomainActive(string name)
        {
            throw new NotImplementedException();
        }

        public string GetDomainsNetworkAddress(string name)
        {
            throw new NotImplementedException();
        }

        public void DeleteMachine(string name)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
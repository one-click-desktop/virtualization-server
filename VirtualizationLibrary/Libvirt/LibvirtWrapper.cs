using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool DoesDomainExist(string name) => GetDomainByName(name) != null;

        public bool DoesDomainActive(string name) => GetDomainByName(name)?.IsActive ?? false;
        
        
        public List<(string mac, string ipv4)> GetDomainsNetworkAddresses(string name)
        {
            //Rozpocząć szukanie od tego!
            //https://libvirt.org/html/libvirt-libvirt-domain.html#virDomainInterfaceAddresses
            return null;
        }
        
        /// <summary>
        /// Metoda niszczy maszynę z libvirta. Nie zajmuje sie żadnymi zasobami z nia powiązanymi!(w przeciwienstwie do vagranta)
        /// </summary>
        /// <param name="name"></param>
        public void DestroyMachine(string name)
        {
            GetDomainByName(name)?.Destroy();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
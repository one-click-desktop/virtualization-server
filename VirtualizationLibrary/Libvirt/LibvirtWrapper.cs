using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
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
        
        #nullable enable
        public LibvirtDomain GetDomainByName(string name) => connection.GetDomainByName(name);

        public bool DoesDomainExist(string name) => GetDomainByName(name) != null;

        public bool DoesDomainActive(string name) => GetDomainByName(name)?.IsActive ?? false;
        
        #nullable enable
        public IEnumerable<IPAddress> GetDomainsNetworkAddresses(string name)
        {
            LibvirtDomain dom = GetDomainByName(name);
            return dom?.GetDomainNetworkAddresses();
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
using System;
using System.Linq;
using IDNT.AppBasics.Virtualization.Libvirt;

namespace OneClickDesktop.VirtualizationLibrary.Test
{
    public class LibvirtHelper : IDisposable
    {
        private LibvirtConnection con;

        public LibvirtHelper(string libvirtUri)
        {
            con = LibvirtConnection.Create.WithLocalAuth().WithMetricsDisabled().Connect();
        }

        public bool IsRunningVm(string name)
        {
            LibvirtDomain dom = con.GetDomainByName(name);
            return dom.IsActive;
        }

        public void Dispose()
        {
            con.Close();
        }
    }
}
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

        private LibvirtDomain GetVmByName(string name)
        {
            foreach (var domain in con.Domains)
                if (domain.Name == name)
                    return domain;
            return null;
        }

        public bool IsRunningVm(string name)
        {
            var dom = GetVmByName(name);
            return dom?.IsActive ?? false;
        }

        public void Dispose()
        {
            con.Close();
        }
    }
}
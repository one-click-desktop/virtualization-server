using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Xml;
using System.Xml.Linq;
using IDNT.AppBasics.Virtualization.Libvirt;
using IDNT.AppBasics.Virtualization.Libvirt.Xml;
using NUnit.Framework;

namespace OneClickDesktop.VirtualizationLibrary.Test
{
    public class LibvirtHelper : IDisposable
    {
        public LibvirtConnection con;

        public LibvirtHelper(string libvirtUri)
        {
            con = LibvirtConnection.Create.WithLocalAuth().WithMetricsDisabled().Connect();
        }

        public bool IsRunningVm(string name) => con.GetDomainByName(name)?.IsActive ?? false;
        public bool Exists(string name) => con.GetDomainByName(name) != null;
 
        public static XDocument GenerateMinimalMachine(string name, string archisoPath = "/tmp/arch.iso", string definitionPath = "res/archiso.xml")
        {
            if (!File.Exists(definitionPath))
                throw new FileNotFoundException("To create machine you need to pass valid path do xml definition.",
                    definitionPath);
            
            string fullArchisoPath = Path.GetFullPath(archisoPath);
            
            //Pobierz obraz, jeżeli go brakuje w testowej ścieżce
            if (!File.Exists(fullArchisoPath))
            {
                Console.WriteLine("Downloading archiso image to {0}", fullArchisoPath);
                WebClient web = new WebClient();
                web.DownloadFile(
                    "http://mirror.rackspace.com/archlinux/iso/2021.12.01/archlinux-2021.12.01-x86_64.iso",
                    fullArchisoPath);
            }
            
            XDocument defintion = XDocument.Load(definitionPath);
            defintion
                .Element("domain")
                .Element("name").Value = name;
            defintion
                .Element("domain")
                .Element("uuid").Value = Guid.NewGuid().ToString();
            defintion
                .Element("domain")
                .Element("devices")
                .Element("disk")
                .Element("source")
                .Attribute("file").Value = fullArchisoPath;

            return defintion;
        }

        public LibvirtDomain CreateTransientMachine(string name)
        {
            XDocument def = GenerateMinimalMachine(name);
            con.CreateDomain(def);
            return con.GetDomainByName(name);
        }

        public LibvirtDomain CreatePersistentMachine(string name)
        {
            XDocument def = GenerateMinimalMachine(name);
            con.DefineDomain(def);
            return con.GetDomainByName(name);
        }

        public void DestroyMachine(params LibvirtDomain[] doms)
        {
            foreach(LibvirtDomain dom in doms)
                dom.Destroy();
        }
        
        public void UndefineMachine(params LibvirtDomain[] doms)
        {
            foreach (LibvirtDomain dom in doms)
                dom.Undefine();
        }
        
        public void StartDefaultNetwork()
        {
           System.Diagnostics.Process.Start("virsh","-c qemu:///system net-start default");
        }
        
        public void Dispose()
        {
            con.Close();
        }
    }
}
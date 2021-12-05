using System;
using System.IO;
using System.Xml.Linq;
using IDNT.AppBasics.Virtualization.Libvirt;
using NUnit.Framework;

namespace OneClickDesktop.VirtualizationLibrary.Test.Libvirt
{
    public class HelperCreationTest
    {
        private LibvirtHelper helper;

        [SetUp]
        public void SetUp()
        {
            string libvirtUri = "qemu:///system";
            helper = new LibvirtHelper(libvirtUri);
        }

        [TearDown]
        public void TearDown()
        {
            helper.Dispose();
        }

        [Test]
        public void CreateSimpleMachine()
        {
            string name = "archtest-1";
            XDocument definition = LibvirtHelper.GenerateMinimalMachine(name);
            
            helper.con.CreateDomain(definition);
            
            Assert.True(helper.IsRunningVm(name));
            
            helper.con.DestroyDomain(helper.con.GetDomainByName(name));
            
            Assert.False(helper.IsRunningVm(name));
        }
    }
}
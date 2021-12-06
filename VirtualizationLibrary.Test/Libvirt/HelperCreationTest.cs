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
            helper.StartDefaultNetwork();
        }

        [TearDown]
        public void TearDown()
        {
            helper.Dispose();
        }
        
        [Test]
        public void CreateTransientMachine()
        {
            string name = "archtest-1";

            var dom = helper.CreateTransientMachine(name);
            
            Assert.True(helper.IsRunningVm(name));

            dom.Destroy();
            
            Assert.False(helper.Exists(name));
        }
        
        [Test]
        public void CreatePersistentMachine()
        {
            string name = "archtest-2";

            var dom = helper.CreatePersistentMachine(name);
            
            Assert.False(helper.IsRunningVm(name));
            Assert.True(helper.Exists(name));

            dom.Undefine();
            
            Assert.False(helper.Exists(name));
        }
    }
}
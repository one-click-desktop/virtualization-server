using System.Reflection;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Libvirt;

namespace OneClickDesktop.VirtualizationLibrary.Test.Libvirt
{
    public class LibvirtWrapperExistenceTest
    {
        private LibvirtWrapper wrapper;
        private LibvirtHelper helper;
        
        [SetUp]
        public void SetUp()
        {
            string libvirtUri = "qemu:///session";
            wrapper = new LibvirtWrapper(libvirtUri);
            helper = new LibvirtHelper(libvirtUri);
            helper.StartDefaultNetwork();
        }

        [TearDown]
        public void Cleanup()
        {
            wrapper.Dispose();
            helper.Dispose();
        }

        [Test]
        public void DoesDomainExists_ExitisingTranscient()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var dom = helper.CreateTransientMachine(name);
            
            Assert.IsTrue(wrapper.DoesDomainExist(name));

            wrapper.DestroyMachine(name);
            
            Assert.IsFalse(wrapper.DoesDomainExist(name));
        }

        [Test]
        public void DoesDomainExists_Inexitising()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Assert.IsFalse(wrapper.DoesDomainExist(name));
        }
        
        [Test]
        public void DoesDomainActive_ExitisingTranscient()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var dom = helper.CreateTransientMachine(name);
            
            Assert.IsTrue(wrapper.DoesDomainExist(name));

            wrapper.DestroyMachine(name);
            
            Assert.IsFalse(wrapper.DoesDomainExist(name));
        }
        
        [Test]
        public void DoesDomainActive_ExitisingPersistent()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var dom = helper.CreatePersistentMachine(name);
            
            Assert.IsFalse(wrapper.DoesDomainActive(name));

            dom.Undefine();
            
            Assert.IsFalse(wrapper.DoesDomainActive(name));
        }

        [Test]
        public void DoesDomainActive_Inexitising()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Assert.IsFalse(wrapper.DoesDomainActive(name));
        }
    }
}
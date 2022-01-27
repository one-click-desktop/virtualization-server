using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Ansible;
using OneClickDesktop.VirtualizationLibrary.Libvirt;
using OneClickDesktop.VirtualizationLibrary.Test.Vagrant;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Libvirt
{
    public class LibvirtWrapperIPTest
    {
        private LibvirtWrapper wrapper;
        private LibvirtHelper helper;
        private VagrantWrapper vagrant;
        private const string bridge = "br0";
        private string libvirtUri = "qemu:///system";

        [SetUp]
        public void SetUp()
        {
            
            string vagrantPath = "res/Vagrantfile";
            wrapper = new LibvirtWrapper(libvirtUri);
            helper = new LibvirtHelper(libvirtUri);
            vagrant = new VagrantWrapper(vagrantPath);
            helper.StartDefaultNetwork();
        }

        [TearDown]
        public void Cleanup()
        {
            wrapper.Dispose();
            helper.Dispose();
        }
        
        /// <summary>
        /// With libvirt creation machine can be at wierd state after ceration. We are reading garbage.
        /// </summary>
        [Theory]
        public void GetAddressesUnbooted()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var dom = helper.CreateTransientMachine(name);

            Assert.That(wrapper.DoesDomainActive(name), Is.True);
            Assume.That(wrapper.GetDomainsNetworkAddresses(name).Count(), Is.EqualTo(0));
            
            dom.Destroy();
            
            Assert.That(wrapper.DoesDomainExist(name), Is.False);
            Assume.That(wrapper.GetDomainsNetworkAddresses(name)?.Count(), Is.Null);
        }
        
        /// <summary>
        /// With vagrant, which is waiting until machine wil get IP Address method will always return good values.
        /// </summary>
        [Theory]
        public void GetAddressesWithBridgeBooted()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            vagrant.VagrantUp(VagrantParametersGenerator.SimpleCreatableAlpine(name, bridge, libvirtUri), new AnsibleParameters());

            Assert.That(wrapper.DoesDomainActive(name), Is.True);
            Assume.That(wrapper.GetDomainsNetworkAddresses(name)?.Count(), Is.EqualTo(5));

            vagrant.VagrantDestroy(VagrantParametersGenerator.SimpleCreatableAlpine(name, bridge, libvirtUri));
            
            Assert.That(wrapper.DoesDomainExist(name), Is.False);
            Assume.That(wrapper.GetDomainsNetworkAddresses(name)?.Count(), Is.Null);
        }
    }
}
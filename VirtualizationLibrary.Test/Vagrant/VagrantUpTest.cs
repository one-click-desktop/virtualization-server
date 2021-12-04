using System.Threading;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public class VagrantUpTest
    {
        private VagrantWrapper wrap;
        private LibvirtHelper helper;
        
        [SetUp]
        public void SetupWrapper()
        {
            string libvirtUri = "qemu:///system";
            helper = new LibvirtHelper(libvirtUri);
            wrap = new VagrantWrapper("res/Vagrantfile");
        }

        ~VagrantUpTest()
        {
            helper.Dispose();
        }
        
        [TestCase]
        public void CreateSimpleAlpine()
        {
            var para = VagrantParametersGenerator.SimpleCreatableAlpine();
            
            //Create machine
            wrap.VagrantUp(para);

            //Check
            bool check = helper.IsRunningVm(para.GetParameter(typeof(NameParameter)));

            //Cleanup
            wrap.VagrantDestroy(para);
            
            Assert.IsTrue(check);
        }
    }
}
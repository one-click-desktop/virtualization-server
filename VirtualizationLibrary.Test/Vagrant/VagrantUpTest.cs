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
            
            wrap = new VagrantWrapper("res/Vagrantfile");
        }
        
        [TestCase]
        public void CreateSimpleAlpine()
        {
            var para = VagrantParametersGenerator.SimpleCreatableAlpine();
            
            //Create machine
            wrap.VagrantUp(para);
            
            //Check
            //tutaj wazny szczegol - gdyby polaczenie trwalo caly czas dostaniemy wyjatek o zlym stanie odmeny
            //Gdy laczymy sie zaraz przed sprawdzeniem to laczenie trwa kilka sekund, ale stany sie zgadzaja
            string libvirtUri = "qemu:///system";
            helper = new LibvirtHelper(libvirtUri);
            bool check = helper.IsRunningVm(para.BoxName);
            
            //Cleanup
            wrap.VagrantDestroy(para);
            
            Assert.IsTrue(check);
        }
    }
}
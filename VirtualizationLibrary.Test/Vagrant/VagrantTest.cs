using System.Threading;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public class VagrantTest
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

        ~VagrantTest()
        {
            helper.Dispose();
        }
        
        [Test]
        public void CreateSimpleAlpine()
        {
            var para = VagrantParametersGenerator.SimpleCreatableAlpine();
            
            //Create machine
            wrap.VagrantUp(para);

            //Check if exists
            Assert.IsTrue(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));

            //Cleanup
            wrap.VagrantDestroy(para);
            
            //Check if doesnt exist
            Assert.IsFalse(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateBadnameAlpine()
        {
            var para = VagrantParametersGenerator.SimpleBadNameAlpine();
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateCpuHungryAlpine()
        {
            var para = VagrantParametersGenerator.SimpleCPUHungryAlpine();
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateMemoryHungryAlpine()
        {
            var para = VagrantParametersGenerator.SimpleMemoryHungryAlpine();
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateInexistingBox()
        {
            var para = VagrantParametersGenerator.InexistingBox();
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));
        }
    }
}
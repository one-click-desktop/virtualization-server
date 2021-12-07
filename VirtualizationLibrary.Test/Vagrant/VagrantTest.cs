using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    /// <summary>
    /// Testy tworzenia maszyn wirtualnych parametryzowanym Vagrantfilem
    /// </summary>
    /// <remarks>
    /// Aby testy działały musi być przygotowane:
    /// 1. Działa libvirtd i uzytkownik ma mozliwosc aby sie podlaczyc do niego (grupa libvirt)
    /// 2. Jest zdefiniowane urzędzenie typu bridge (zmienic nazwe mozna w setupie)
    /// </remarks>
    public class VagrantTest
    {
        private VagrantWrapper wrap;
        private LibvirtHelper helper;
        private string testBridgeDevice;
        
        [SetUp]
        public void SetupWrapper()
        {
            //Zmienić przed uruchomieniem na nowym komputerze! (Czy jakiś bridge na pewno istnieje?)
            testBridgeDevice = "br0";
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
            var para = VagrantParametersGenerator.SimpleCreatableAlpine(MethodBase.GetCurrentMethod().Name, testBridgeDevice);
            
            //Create machine
            wrap.VagrantUp(para);

            //Check if exists
            Assert.IsTrue(helper.IsRunningVm(para.GetParameterValue(typeof(NameParameter))));

            //Cleanup
            wrap.VagrantDestroy(para);
            
            //Check if doesnt exist
            Assert.IsFalse(helper.Exists(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateBadnameAlpine()
        {
            var para = VagrantParametersGenerator.SimpleBadNameAlpine(MethodBase.GetCurrentMethod().Name, testBridgeDevice);
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.Exists(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateCpuHungryAlpine()
        {
            var para = VagrantParametersGenerator.SimpleCPUHungryAlpine(MethodBase.GetCurrentMethod().Name, testBridgeDevice);
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.Exists(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateMemoryHungryAlpine()
        {
            var para = VagrantParametersGenerator.SimpleMemoryHungryAlpine(MethodBase.GetCurrentMethod().Name, testBridgeDevice);
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.Exists(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateUnexistingBox()
        {
            var para = VagrantParametersGenerator.UnexistingBox(MethodBase.GetCurrentMethod().Name, testBridgeDevice);
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.Exists(para.GetParameterValue(typeof(NameParameter))));
        }
        
        [Test]
        public void CreateBoxWithUnexistingBridge()
        {
            var para = VagrantParametersGenerator.SimpleCreatableAlpine(MethodBase.GetCurrentMethod().Name, "ijgsahdfiuqgfyuwkegfouwafbsuhafiw");
            
            //Create machine
            Assert.Catch<VagrantException>(() => wrap.VagrantUp(para));

            //Check if doesnt exist
            Assert.IsFalse(helper.Exists(para.GetParameterValue(typeof(NameParameter))));
        }
    }
}
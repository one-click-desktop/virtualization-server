using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public class VagrantParametersTest
    {
        [TestCase]
        public void TestFormatForExecute()
        {
            string vm_name = "test_name";
            string hostname = "test_hostname";
            int cpus = 6;
            int memory = 1234;
            string box = "generic/alpine38";
            var p = new VagrantParameters(box, vm_name, hostname, memory, cpus);

            string expected = $"--boxname=\"{box}\" --vm-name=\"{vm_name}\" --cpus=\"{cpus}\" --memory=\"{memory}\" --hostname=\"{hostname}\"";
            string ret = p.FormatForExecute();
            
            StringAssert.AreEqualIgnoringCase(expected, ret);
        }
    }
}
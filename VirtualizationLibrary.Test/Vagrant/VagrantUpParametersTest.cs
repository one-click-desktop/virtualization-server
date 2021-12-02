using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public class VagrantUpParametersTest
    {
        [TestCase]
        public void TestFormatForExecute()
        {
            string vm_name = "test_name";
            string hostname = "test_hostname";
            int cpus = 6;
            int memory = 1234;
            var p = new VagrantUpParameters(vm_name, hostname, memory, cpus);

            string expected = $"--vm-name={vm_name} --cpus={cpus} --memory={memory} --hostname={hostname}";
            string ret = p.FormatForExecute();
            
            StringAssert.AreEqualIgnoringCase(expected, ret);
        }
    }
}
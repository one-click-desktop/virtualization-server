using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public class VagrantParametersTest
    {
        [TestCase]
        public void EnvironmentParametrsSet()
        {
            string vm_name = "test_name";
            string hostname = "test_hostname";
            int cpus = 6;
            int memory = 1234;
            string box = "generic/alpine38";
            var p = new VagrantParameters(box, vm_name, hostname, memory, cpus);

            StringDictionary env = new StringDictionary();
            env.Add("TEST", "TEST");
            p.DefineEnvironmentalVariables(env);
            
            StringAssert.AreEqualIgnoringCase(vm_name, env["OCD_VMNAME"]);
            StringAssert.AreEqualIgnoringCase(hostname, env["OCD_HOSTNAME"]);
            StringAssert.AreEqualIgnoringCase(box, env["OCD_BOXNAME"]);
            StringAssert.AreEqualIgnoringCase(cpus.ToString(), env["OCD_CPUS"]);
            StringAssert.AreEqualIgnoringCase(memory.ToString(), env["OCD_MEMORY"]);
            StringAssert.AreEqualIgnoringCase("TEST", env["TEST"]);
        }
    }
}
using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public static class VagrantParametersGenerator
    {
        public static VagrantParameters SimpleCreatableAlpine()
        {
            return new VagrantParameters("generic/alpine38", "testname", "testhostname", 512, 2);
        }
        
        public static VagrantParameters SimpleBadNameAlpine()
        {
            return new VagrantParameters("generic/alpine38", "test_name", "test_hostname", 512, 2);
        }
        
        public static VagrantParameters SimpleMemoryHungryAlpine()
        {
            return new VagrantParameters("generic/alpine38", "testname", "testhostname", 123512, 2);
        }
        
        public static VagrantParameters SimpleCPUHungryAlpine()
        {
            return new VagrantParameters("generic/alpine38", "testname", "testhostname", 512, 1234);
        }
    }
}
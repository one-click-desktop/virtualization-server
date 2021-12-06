using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public static class VagrantParametersGenerator
    {
        public static VagrantParameters SimpleCreatableAlpine(string name, string bridge)
        {
            return new VagrantParameters("generic/alpine38", name, "testhostname", bridge, 512, 2);
        }
        
        public static VagrantParameters SimpleBadNameAlpine(string name, string bridge)
        {
            return new VagrantParameters("generic/alpine38", "_"+name, "test_hostname", bridge, 512, 2);
        }
        
        public static VagrantParameters SimpleMemoryHungryAlpine(string name, string bridge)
        {
            return new VagrantParameters("generic/alpine38", name, "testhostname", bridge, 123512, 2);
        }
        
        public static VagrantParameters SimpleCPUHungryAlpine(string name, string bridge)
        {
            return new VagrantParameters("generic/alpine38", name, "testhostname", bridge, 512, 1234);
        }
        
        public static VagrantParameters UnexistingBox(string name, string bridge)
        {
            return new VagrantParameters("auhoiuhgkuywqfriuywkweefduyw", name, "testhostname", bridge, 512, 2);
        }
    }
}
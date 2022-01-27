using OneClickDesktop.VirtualizationLibrary.Vagrant;

namespace OneClickDesktop.VirtualizationLibrary.Test.Vagrant
{
    public static class VagrantParametersGenerator
    {
        public static VagrantParameters SimpleCreatableAlpine(string name, string bridge, string libvirtUri)
        {
            return new VagrantParameters("generic/alpine38", name, "testhostname", bridge, 512, 2, "", libvirtUri, "",
                "");
        }

        public static VagrantParameters SimpleBadNameAlpine(string name, string bridge, string libvirtUri)
        {
            return new VagrantParameters("generic/alpine38", "_" + name, "test_hostname", bridge, 512, 2,
                "res/poststartup_playbook.yml", libvirtUri, "",
                "");
        }

        public static VagrantParameters SimpleMemoryHungryAlpine(string name, string bridge, string libvirtUri)
        {
            return new VagrantParameters("generic/alpine38", name, "testhostname", bridge, 123512, 2,
                "res/poststartup_playbook.yml", libvirtUri, "",
                "");
        }

        public static VagrantParameters SimpleCPUHungryAlpine(string name, string bridge, string libvirtUri)
        {
            return new VagrantParameters("generic/alpine38", name, "testhostname", bridge, 512, 1234,
                "res/poststartup_playbook.yml", libvirtUri, "",
                "");
        }

        public static VagrantParameters UnexistingBox(string name, string bridge, string libvirtUri)
        {
            return new VagrantParameters("auhoiuhgkuywqfriuywkweefduyw", name, "testhostname", bridge, 512, 2,
                "res/poststartup_playbook.yml", libvirtUri, "",
                "");
        }
    }
}
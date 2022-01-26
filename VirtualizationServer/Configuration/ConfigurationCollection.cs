namespace OneClickDesktop.VirtualizationServer.Configuration
{
    public class ConfigurationCollection
    {
        public ResourcesConfiguration ResourceConfiguration { get; set; }
        public VirtSrvConfiguration VirtSrvConfiguration { get; set; }
        public NfsConfiguration NfsConfiguration { get; set; }
        public LdapConfiguration LdapConfiguration { get; set; }
    }
}
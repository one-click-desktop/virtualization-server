namespace OneClickDesktop.VirtualizationServer.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class LdapConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public string Uri { get; set; } = "ldap://localhost";
        /// <summary>
        /// 
        /// </summary>
        public string Domain { get; set; } = "dc=example,dc=org";
        /// <summary>
        /// 
        /// </summary>
        public string ReadOnlyDn { get; set; } = "cn=readonly,dc=example,dc=org";
        /// <summary>
        /// 
        /// </summary>
        public string ReadOnlyPassword { get; set; } = "readonly";
        /// <summary>
        /// 
        /// </summary>
        public string AdminDn { get; set; } = "cn=admin,dc=example,dc=org";
        /// <summary>
        /// 
        /// </summary>
        public string GroupsDn { get; set; } = "ou=groups,dc=example,dc=org";
        /// <summary>
        /// 
        /// </summary>
        public string UsersDn { get; set; } = "ou=users,dc=example,dc=org";
    }
}
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OneClickDesktop.VirtualizationLibrary.Ansible
{
    public abstract class AbstractParameter
    {
        protected const string ENV_PREFIX = "OCD_";

        /// <summary>
        /// Nazwa zmiennej srodowiskowej na ktorej powinna znalezc sie wartosc parametru
        /// </summary>
        public virtual string EnvironmentVariable => ENV_PREFIX + envSuffix;

        /// <summary>
        /// Wartośc parametru
        /// </summary>
        public string Value => value;

        protected string value;
        protected string envSuffix;

        public AbstractParameter(string value, string envSuffix)
        {
            this.value = value;
            this.envSuffix = envSuffix;
        }

        /// <summary>
        /// Ustawia zmienna środowiskowa z waroytścią parametru do Vagrantfile
        /// </summary>
        /// <param name="env">Słownik reprezentujący środowisko</param>
        public virtual void SetEnironmentalVariable(StringDictionary env)
        {
            env[EnvironmentVariable] = value;
        }
    }

    public class LdapUriParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_URI";

        public LdapUriParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class LdapDomainParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_DOMAIN";

        public LdapDomainParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class LdapReadOnlyDnParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_RODN";

        public LdapReadOnlyDnParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class LdapReadOnlyPasswordParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_ROPW";

        public LdapReadOnlyPasswordParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class LdapAdminDnParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_ADMDN";

        public LdapAdminDnParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class LdapGroupsDnParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_GRPDN";

        public LdapGroupsDnParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class LdapUsersDnParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "LDAP_USRDN";

        public LdapUsersDnParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class NfsServerNameParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "NFS_SRVNAME";

        public NfsServerNameParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class NfsHomePathParameter : AbstractParameter
    {
        public const string ENV_SUFFIX = "NFS_HOME_PATH";

        public NfsHomePathParameter(string value) : base(value, ENV_SUFFIX)
        {
        }
    }

    public class AnsibleParameters
    {
        private List<AbstractParameter> parameters;

        public AnsibleParameters()
        {
            parameters = new List<AbstractParameter>();
        }
        
        public AnsibleParameters(string ldapUri, string ldapDomain, string ldapRodn, string ldapRopw, string ldapAdmdn,
            string ldapGrpdn, string ldapUsrdn, string nfsSrvname, string nfsHome)
        {
            parameters = new List<AbstractParameter>()
            {
                new LdapUriParameter(ldapUri),
                new LdapDomainParameter(ldapDomain),
                new LdapReadOnlyDnParameter(ldapRodn),
                new LdapReadOnlyPasswordParameter(ldapRopw),
                new LdapAdminDnParameter(ldapAdmdn),
                new LdapGroupsDnParameter(ldapGrpdn),
                new LdapUsersDnParameter(ldapUsrdn),
                new NfsServerNameParameter(nfsSrvname),
                new NfsHomePathParameter(nfsHome)
            };
        }
        
        public void DefineEnvironmentalVariables(StringDictionary env)
        {
            foreach (AbstractParameter parameter in parameters)
                parameter.SetEnironmentalVariable(env);
        }
    }
}
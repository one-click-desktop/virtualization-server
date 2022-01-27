using System.Collections.Specialized;
using NUnit.Framework;
using OneClickDesktop.VirtualizationLibrary.Ansible;

namespace OneClickDesktop.VirtualizationLibrary.Test.Ansible
{
    public class AnsibleParametersTest
    {
        [Test]
        public void EnvironmentParametrsSet()
        {
            string lUri = "ldap://test";
            string lDomain = "test_domain";
            string lROdn = "rodn";
            string lROpw = "ropw";
            string lAdn = "lAdm";
            string lgdn = "lGdn";
            string ludn = "ludn";
            string nSrv = "nsrv";
            string nHome = "nhome";
            var p = new AnsibleParameters(lUri, lDomain, lROdn, lROpw, lAdn, lgdn, ludn, nSrv, nHome);

            StringDictionary env = new StringDictionary();
            env.Add("TEST", "TEST");
            p.DefineEnvironmentalVariables(env);
            
            StringAssert.AreEqualIgnoringCase(lUri, env["OCD_LDAP_URI"]);
            StringAssert.AreEqualIgnoringCase(lDomain, env["OCD_LDAP_DOMAIN"]);
            StringAssert.AreEqualIgnoringCase(lROdn, env["OCD_LDAP_RODN"]);
            StringAssert.AreEqualIgnoringCase(lROpw, env["OCD_LDAP_ROPW"]);
            StringAssert.AreEqualIgnoringCase(lAdn, env["OCD_LDAP_ADMDN"]);
            StringAssert.AreEqualIgnoringCase(lgdn, env["OCD_LDAP_GRPDN"]);
            StringAssert.AreEqualIgnoringCase(ludn, env["OCD_LDAP_USRDN"]);
            StringAssert.AreEqualIgnoringCase(nSrv, env["OCD_NFS_SRVNAME"]);
            StringAssert.AreEqualIgnoringCase(nHome, env["OCD_NFS_HOME_PATH"]);
            StringAssert.AreEqualIgnoringCase("TEST", env["TEST"]);
        }
    }
}
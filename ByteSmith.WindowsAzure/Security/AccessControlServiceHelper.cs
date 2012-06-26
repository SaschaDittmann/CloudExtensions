using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ByteSmith.WindowsAzure.Security
{
    public class AccessControlServiceHelper
    {
        public static List<IdentityProvider> GetIdentityProviders(string acsNamespace, string realm)
        {
            return GetIdentityProviders(acsNamespace, realm, "wsfederation", "", "");
        }

        public static List<IdentityProvider> GetIdentityProviders(string acsNamespace, string realm, string protocol)
        {
            return GetIdentityProviders(acsNamespace, realm, protocol, "", "");
        }

        public static List<IdentityProvider> GetIdentityProviders(string acsNamespace, string realm, string protocol, string context)
        {
            return GetIdentityProviders(acsNamespace, realm, protocol, context, "");
        }

        public static List<IdentityProvider> GetIdentityProviders(string acsNamespace, string realm, string protocol, string context, string requestId)
        {
            var url = string.Format(
                "https://{0}.accesscontrol.windows.net:443/v2/metadata/IdentityProviders.js?protocol={1}&realm={2}&context={3}&request_id={4}&version=1.0",
                acsNamespace,
                protocol,
                HttpUtility.UrlEncode(realm),
                context,
                requestId);
 
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var json = client.DownloadString(url);

            return JsonConvert.DeserializeObject<List<IdentityProvider>>(json);
        }
    }
}

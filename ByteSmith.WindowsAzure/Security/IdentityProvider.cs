using System.Collections.Generic;

namespace ByteSmith.WindowsAzure.Security
{
    public class IdentityProvider
    {
        public string Name { get; set; }
        public string LoginUrl { get; set; }
        public string LogoutUrl { get; set; }
        public string ImageUrl { get; set; }
        public List<string> EmailAddressSuffixes { get; set; }
    }
}

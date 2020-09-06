using Microsoft.Extensions.Configuration;

namespace azure_key_vault
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string KeyVaultBaseUri { get; set; }

        public string AppClientScope { get; set; }
        public string Instance { get; set; }
        public string SecretName { get; set; }

        public static Settings LoadFromConfiguration(IConfiguration config)
        {
            return config.Get<Settings>();
        }
    }
}
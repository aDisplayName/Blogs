using System;

namespace azure_key_vault
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Tenantid { get; set; }
        public string KeyVaultBaseUri { get; set; }

        public string AppClientScope { get; set; }
        public string Instance { get; set; }
        public string SecretName { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}

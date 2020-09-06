using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
namespace azure_key_vault
{
    public static class Program
    {
        // In on-behalf-of flow, the following scope needs to be consented when acquiring the user token. Otherwise, the 
        // app cannot access the key vault on-behalf-of user.
        const string KeyVaultUserImScope = "https://vault.azure.net/user_impersonation";
        // In on-behalf-of flow, the following scope is used when acquiring client token
        const string KeyVaultScope = "https://vault.azure.net/.default";

        private static async Task Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddCommandLine(args)
                    .AddEnvironmentVariables()
                    .Build();

                var setting = Settings.LoadFromConfiguration(config);

                // await ClientAccess(setting);
                // await UserAccess(setting);
                await CompoundAccess(setting);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        
        /// <summary>
        /// Retrieve Key Vault Secret using App Service Principal
        /// </summary>
        /// <returns></returns>
        private static async Task ClientAccess(Settings settings)
        {

            // This is the client secret from the app registration process.
            // This is available as "DNS Name" from the overview page of the Key Vault.
            var client = new SecretClient(new Uri(settings.KeyVaultBaseUri),
                new ClientSecretCredential(settings.TenantId, settings.ClientId, settings.ClientSecret));

            // Calling GetSecretAsync will trigger the authentication code above and eventually
            // retrieve the secret which we can then read.
            var secretBundle = await client.GetSecretAsync(settings.SecretName);
            Console.WriteLine("Secret:" + secretBundle.Value.Value);
            Console.ReadKey();
        }

        /// <summary>
        /// Retrieve Key Vault data using user service principal
        /// </summary>
        /// <returns></returns>
        private static async Task UserAccess(Settings settings)
        {
            // This is available as "DNS Name" from the overview page of the Key Vault.
            var client = new SecretClient(new Uri(settings.KeyVaultBaseUri), new InteractiveBrowserCredential());

            // Calling GetSecretAsync will trigger the authentication code above and eventually
            // retrieve the secret which we can then read.
            var secretBundle = await client.GetSecretAsync(settings.SecretName);
            Console.WriteLine("Secret:" + secretBundle.Value.Value);
            Console.ReadKey();
        }

        /// <summary>
        /// Retrieve Key Vault data using Compound Identity (On-Behalf-Of)
        /// </summary>
        /// <returns></returns>
        private static async Task CompoundAccess(Settings settings)
        {
            // When using CompoundAccess, the consent to the 

            Console.WriteLine("Acquire User token");
            var clientApp = PublicClientApplicationBuilder.Create(settings.ClientId)
                .WithAuthority($"{settings.Instance}{settings.TenantId}")
                .WithRedirectUri("http://localhost")    // Make sure the "http://localhost" is added and selected as the app Redirect URI
                .Build();

            
            var resultUser = clientApp
                .AcquireTokenInteractive(new[] { settings.AppClientScope })    // Make sure the same scope name is created in "Exposed API" section for this app registration in azure portal
                .WithExtraScopesToConsent(new [] {KeyVaultUserImScope})
                .WithPrompt(Prompt.Consent)
                .ExecuteAsync().Result;

            Console.WriteLine("Acquire Client token");
            var clientApp2 = ConfidentialClientApplicationBuilder.Create(settings.ClientId)
                .WithAuthority($"{settings.Instance}{settings.TenantId}")
                .WithClientSecret(settings.ClientSecret)
                .Build();

            
            var resultObo = clientApp2
                .AcquireTokenOnBehalfOf(
                    new[] {KeyVaultScope},
                    new UserAssertion(resultUser.AccessToken))
                .ExecuteAsync().Result;

            Console.WriteLine("Access Key Vault");
            var kc = new KeyVaultCredential(
                (authority, resource, scope) =>
                {
                    Console.WriteLine($"Authority: {authority}, Resource: {resource}, Scope: {scope}");
                    return Task.FromResult(resultObo.AccessToken);
                });

            var kvClient = new KeyVaultClient(kc);
            var secretBundle = await kvClient.GetSecretAsync(settings.KeyVaultBaseUri, settings.SecretName);

            Console.WriteLine("Secret:" + secretBundle.Value);
        }

    }
}

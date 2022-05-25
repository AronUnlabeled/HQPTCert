using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using OfficeDevPnP.Core;
using AuthenticationManager = OfficeDevPnP.Core.AuthenticationManager;

namespace ClassLibrary2
{
    public class Class1
    {

        static void Main(string[] args)
        {

            using (var cc = new AuthenticationManager().GetAzureADAppOnlyAuthenticatedContext(
                "https://qf06.sharepoint.com/sites/PNCSageCSVConversion",
                "0mddc88c6e-3f2f-4a07-a7c3-0ff1b242847d",
                "qf06.onmicrosoft.com",
                GetKeyVaultCertificate("HQPT-Vault", "HQPT-Func")))
            {
                cc.Load(cc.Web, p => p.Title);
                cc.ExecuteQuery();
            };
        }


        internal static X509Certificate2 GetKeyVaultCertificate(string keyvaultName, string name)
        {
            // Some steps need to be taken to make this work
            // 1. Create a KeyVault and upload the certificate
            // 2. Give the Function App the permission to GET certificates via Access Policies in the KeyVault
            // 3. Call an explicit access token request to the management resource to https://vault.azure.net and use the URL of our Keyvault in the GetSecretMethod

            //KeyVaultClient keyVaultClient = new KeyVaultClient;
            //if (keyVaultClient == null)
            //{
            // this token provider gets the appid/secret from the azure function identity
            //and thus makes the call on behalf of that appid/secret
            var serviceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));
            //}

            // Getting the certificate
            var secret = keyVaultClient.GetSecretAsync("https://" + keyvaultName + ".vault.azure.net/", name);

            // Returning the certificate
            return new X509Certificate2(Convert.FromBase64String(secret.Result.Value));

            // If you receive the following error when running the Function;
            // Microsoft.Azure.WebJobs.Host.FunctionInvocationException:
            // Exception while executing function: NotificationFunctions.QueueOperation--->
            // System.Security.Cryptography.CryptographicException:
            // The system cannot find the file specified.at System.Security.Cryptography.NCryptNative.ImportKey(SafeNCryptProviderHandle provider, Byte[] keyBlob, String format) at System.Security.Cryptography.CngKey.Import(Byte[] keyBlob, CngKeyBlobFormat format, CngProvider provider)
            //
            // Please see https://stackoverflow.com/questions/31685278/create-a-self-signed-certificate-in-net-using-an-azure-web-application-asp-ne
            // Add the following Application setting to the AF "WEBSITE_LOAD_USER_PROFILE = 1"
        }


    }
}

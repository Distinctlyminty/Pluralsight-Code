using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace ManagingMediaAssets
{
    class MainClass
    {
        private const string InputMP4FileName = @"DemoIntro.mp4";

        public static async Task Main(string[] args)
        {
            ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build());

            try
            {
                await RunAsync(config);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"{exception.Message}");

                if (exception.GetBaseException() is ApiErrorException apiException)
                {
                    Console.Error.WriteLine(
                        $"ERROR: API call failed with error code '{apiException.Body.Error.Code}' and message '{apiException.Body.Error.Message}'.");
                }
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }

        private static async Task RunAsync(ConfigWrapper config)
        {
            IAzureMediaServicesClient client;
            try
            {
                client = await CreateMediaServicesClientAsync(config);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e.Message}");
                return;
            }
            
            // Creating a unique suffix so that we don't have name collisions 
            string uniqueness = Guid.NewGuid().ToString("N");
            string inputAssetName = $"input-{uniqueness}";


            // Create a new input Asset and upload the specified local video file into it.
            await CreateInputAssetAsync(client, config.ResourceGroup, config.AccountName, inputAssetName, InputMP4FileName);

            // list all of our Assets
            await ListAssetsAsync(client, config.ResourceGroup, config.AccountName);


            await CleanUpAsync(client, config.ResourceGroup, config.AccountName, inputAssetName);

        }

        private static async Task ListAssetsAsync(IAzureMediaServicesClient client, string resourceGroup, string accountName)
        {
            var assets = await client.Assets.ListAsync(resourceGroup, accountName);
            foreach (var asset in assets)
            {
                Console.WriteLine($"Asset ID: {asset.AssetId} Asset Name: {asset.Name}");
            }
        }

        private static async Task<Asset> CreateInputAssetAsync(
           IAzureMediaServicesClient client,
           string resourceGroupName,
           string accountName,
           string assetName,
           string fileToUpload)
        {
            Console.WriteLine("Creating an Input Asset");
            Asset asset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);

            if (asset == null)
            {
                // Call Media Services API to create an Asset.
                // This method creates a container in storage for the Asset.
                // The files (blobs) associated with the asset will be stored in this container.
                asset = await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());

            }
            else
            {
                Console.WriteLine($"Warning: The asset named {assetName} already exists. It will be overwritten.");
            }

            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            var response = await client.Assets.ListContainerSasAsync(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            // Use Storage API to get a reference to the Asset container
            // that was created by calling Asset's CreateOrUpdate method.  
            CloudBlobContainer container = new CloudBlobContainer(sasUri);
            var blob = container.GetBlockBlobReference(Path.GetFileName(fileToUpload));

            // Use Storage API to upload the file into the container in storage.
            await blob.UploadFromFileAsync(fileToUpload);

            return asset;
        }

        private static async Task CleanUpAsync(
         IAzureMediaServicesClient client,
         string resourceGroupName,
         string accountName,
         string inputAssetName)
        {
            Console.WriteLine("Cleaning up...");
            Console.WriteLine();

            await client.Assets.DeleteAsync(resourceGroupName, accountName, inputAssetName);
        }

        private static async Task<ServiceClientCredentials> GetCredentialsAsync(ConfigWrapper config)
        {
            ClientCredential clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }

        private static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
        {
            var credentials = await GetCredentialsAsync(config);

            return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId,
            };
        }

      
    }
}

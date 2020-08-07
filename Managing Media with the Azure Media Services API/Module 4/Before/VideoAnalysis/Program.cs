using System;
using System.Collections.Generic;
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

namespace AnalyzeVideo
{
    public class Program
    {
        private const string AdaptiveStreamingTransformName = "VideoAnalysisTransformAudioOnly";

        // Don't forget to add a video file of your own!
        private const string InputMP4FileName = @"Add your file name here";

        private const string OutputAssetsFolder = @"OutputAssets";

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
            IAzureMediaServicesClient client = await CreateMediaServicesClientAsync(config);

            client.LongRunningOperationRetryTimeout = 2;

            string uniqueness = Guid.NewGuid().ToString("N");
            string jobName = $"job-{uniqueness}";
            string locatorName = $"locator-{uniqueness}";
            string outputAssetName = $"output-{uniqueness}";
            string inputAssetName = $"input-{uniqueness}";

            Transform transform = await GetOrCreateTransformAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName);

            await CreateInputAssetAsync(client, config.ResourceGroup, config.AccountName, inputAssetName, InputMP4FileName);

            Asset outputAsset = await CreateOutputAssetAsync(client, config.ResourceGroup, config.AccountName, outputAssetName);

            Job job = await SubmitJobAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, jobName, inputAssetName, outputAsset.Name);
           
            job = await WaitForJobToFinishAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, jobName);

            if (job.State == JobState.Finished)
            { 
                Console.WriteLine("Job finished.");
                if (!Directory.Exists(OutputAssetsFolder))
                    Directory.CreateDirectory(OutputAssetsFolder);

                await DownloadOutputAssetAsync(client, config.ResourceGroup, config.AccountName, outputAsset.Name,
                    OutputAssetsFolder);
            }

            Console.WriteLine("Done. Copy and paste the Streaming URL into a browser.");
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
       
        private static async Task<Asset> CreateInputAssetAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string assetName,
            string fileToUpload)
        {
           
            Asset asset = await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());

            var response = await client.Assets.ListContainerSasAsync(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            CloudBlobContainer container = new CloudBlobContainer(sasUri);
            var blob = container.GetBlockBlobReference(Path.GetFileName(fileToUpload));

            await blob.UploadFromFileAsync(fileToUpload);

            return asset;
        }
       
        private static async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName)
        {
            Asset outputAsset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);
            Asset asset = new Asset();
            string outputAssetName = assetName;

            if (outputAsset != null)
            {
                string uniqueness = $"-{Guid.NewGuid().ToString("N")}";
                outputAssetName += uniqueness;

                Console.WriteLine("Warning – found an existing Asset with name = " + assetName);
                Console.WriteLine("Creating an Asset with this name instead: " + outputAssetName);
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, outputAssetName, asset);
        }
       
        private static async Task<Transform> GetOrCreateTransformAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName)
        {
            
            Transform transform = await client.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                TransformOutput[] output = new TransformOutput[]
                {
                    new TransformOutput

                    {
                     
                     
                    }
                };
         
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, output);
            }

            return transform;
        }
        
        private static async Task<Job> SubmitJobAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName,
            string inputAssetName,
            string outputAssetName)
        {
            JobInput jobInput = new JobInputAsset(assetName: inputAssetName);

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            Job job = await client.Jobs.CreateAsync(
                resourceGroupName,
                accountName,
                transformName,
                jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            return job;
        }
       
        private static async Task<Job> WaitForJobToFinishAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName)
        {
            const int SleepIntervalMs = 20 * 1000;

            Job job = null;

            do
            {
                job = await client.Jobs.GetAsync(resourceGroupName, accountName, transformName, jobName);
                
                Console.WriteLine($"Job is '{job.State}'.");
                
                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }

        private static async Task DownloadOutputAssetAsync(
             IAzureMediaServicesClient client,
             string resourceGroup,
             string accountName,
             string assetName,
             string outputFolderName)
        {

        }

    }
}
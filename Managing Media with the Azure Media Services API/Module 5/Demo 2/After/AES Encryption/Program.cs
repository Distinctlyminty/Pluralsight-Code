using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace AESEncryption
{
    public class Program
    {
        private const string AdaptiveStreamingTransformName = "AdaptiveStreamingTransform";
        private const string InputMP4FileName = @"DemoIntro.mp4";
        private const string OutputAssetsFolder = @"OutputAssets";
        private static readonly string Issuer = "Globmantics";
        private static readonly string Audience = "Consumer";
        private static byte[] TokenSigningKey = new byte[40];
        private static readonly string ContentKeyPolicyName = "AESContentKeyPolicy";
        private static readonly string DefaultStreamingEndpointName = "default";

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

            bool stopEndpoint = false;

            try
            {

                Transform transform = await GetOrCreateTransformAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName);

                await CreateInputAssetAsync(client, config.ResourceGroup, config.AccountName, inputAssetName, InputMP4FileName);

                Asset outputAsset = await CreateOutputAssetAsync(client, config.ResourceGroup, config.AccountName, outputAssetName);

                Job job = await SubmitJobAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, jobName, inputAssetName, outputAsset.Name);

                job = await WaitForJobToFinishAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, jobName);

                if (job.State == JobState.Finished)
                {
                    Console.WriteLine("Job finished.");

                    // Create a TokenSigningKey.
                    TokenSigningKey = Convert.FromBase64String(config.SymmetricKey);


                    // Create the content key policy
                    ContentKeyPolicy policy = await GetOrCreateContentKeyPolicyAsync(client, config.ResourceGroup, config.AccountName, ContentKeyPolicyName, TokenSigningKey);

                    StreamingLocator locator = await CreateStreamingLocatorAsync(client, config.ResourceGroup, config.AccountName, outputAsset.Name, locatorName, ContentKeyPolicyName);

                    StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(config.ResourceGroup,
                        config.AccountName, DefaultStreamingEndpointName);

                    if (streamingEndpoint != null)
                    {
                        if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                        {
                            await client.StreamingEndpoints.StartAsync(config.ResourceGroup, config.AccountName, DefaultStreamingEndpointName);

                            // Since we started the endpoint, we should stop it in cleanup.
                            stopEndpoint = true;
                        }
                    }
                    // Ge the Dash streaming URL path

                    var dashPath = await GetStreamingUrlAsync(client, config.ResourceGroup, config.AccountName, locatorName, streamingEndpoint);


                    // Get the key identifier of the content.
                    string keyIdentifier = locator.ContentKeys.First().Id.ToString();

                    Console.WriteLine($"KeyIdentifier = {keyIdentifier}");

                    //  To generate our test token we must get the ContentKeyId to put in the ContentKeyIdentifierClaim claim.
                    string token = GetTokenAsync(Issuer, Audience, keyIdentifier, TokenSigningKey);

                    Console.WriteLine("Copy and paste the following URL in your browser to play back the file in the Azure Media Player.");
                    Console.WriteLine("Note, the player is set to use the AES token and the Bearer token is specified. ");
                    Console.WriteLine();
                    Console.WriteLine($"https://ampdemo.azureedge.net/?url={dashPath}&aes=true&aestoken=Bearer%3D{token}");
                    Console.WriteLine();

                    Console.WriteLine("Press enter to cleanup.");
                    Console.Out.Flush();
                    Console.ReadLine();

                }

            }
            catch (ApiErrorException e)
            {
                Console.WriteLine("EXCEPTION");
                Console.WriteLine($"\tCode: {e.Body.Error.Code}");
                Console.WriteLine($"\tMessage: {e.Body.Error.Message}");
                Console.WriteLine();
                Console.WriteLine("Exiting, cleanup may be necessary...");
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("Cleaning up...");
                await CleanUpAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, locatorName, outputAssetName,
                    jobName, ContentKeyPolicyName, stopEndpoint, DefaultStreamingEndpointName);

                // to remove everything uncomment this line - use with caution as it will remove everything!
                // await CleanUpEverything(client, config.ResourceGroup, config.AccountName);


            }

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
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
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

        private static async Task<StreamingLocator> CreateStreamingLocatorAsync(
            IAzureMediaServicesClient client,
            string resourceGroup,
            string accountName,
            string assetName,
            string locatorName,
            string contentPolicyName)
        {
            StreamingLocator locator = await client.StreamingLocators.GetAsync(resourceGroup, accountName, locatorName);

            locator = await client.StreamingLocators.CreateAsync(
                resourceGroup,
                accountName,
                locatorName,
                new StreamingLocator
                {
                    AssetName = assetName,
                    StreamingPolicyName = PredefinedStreamingPolicy.ClearKey,
                    DefaultContentKeyPolicyName = contentPolicyName
                });

            return locator;
        }

       // Create the token that's used to protect the stream
        private static string GetTokenAsync(string issuer, string audience, string keyIdentifier, byte[] tokenVerificationKey)
        {
            var tokenSigningKey = new SymmetricSecurityKey(tokenVerificationKey);

            SigningCredentials cred = new SigningCredentials(
                tokenSigningKey,
                // Use the  HmacSha256 and not the HmacSha256Signature option, or the token will not work!
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest);

            Claim[] claims = new Claim[]
            {
                new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, keyIdentifier)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.Now.AddMinutes(-5),
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: cred);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }

      
        // Check the Streaming Endpoint is running and retrieve the DASH URL
        private static async Task<string> GetStreamingUrlAsync(IAzureMediaServicesClient client, string resourceGroupName,
            string accountName, string locatorName, StreamingEndpoint streamingEndpoint)
        {
            string dashPath = "";

            ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);

            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new UriBuilder
                {
                    Scheme = "https",
                    Host = streamingEndpoint.HostName
                };

                if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                {
                    uriBuilder.Path = path.Paths[0];
                    dashPath = uriBuilder.ToString();
                }
            }

            return dashPath;
        }

      
        // Create the content key policy that configures how the content key is delivered to end clients 
        private static async Task<ContentKeyPolicy> GetOrCreateContentKeyPolicyAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string contentKeyPolicyName,
            byte[] tokenSigningKey)
        {
            ContentKeyPolicy policy = await client.ContentKeyPolicies.GetAsync(resourceGroupName, accountName, contentKeyPolicyName);

            if (policy == null)
            {
                ContentKeyPolicySymmetricTokenKey primaryKey = new ContentKeyPolicySymmetricTokenKey(TokenSigningKey);
                List<ContentKeyPolicyRestrictionTokenKey> alternateKeys = null;
                List<ContentKeyPolicyTokenClaim> requiredClaims = new List<ContentKeyPolicyTokenClaim>()
                {
                    ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim
                };

                List<ContentKeyPolicyOption> options = new List<ContentKeyPolicyOption>()
                {
                    new ContentKeyPolicyOption(
                        new ContentKeyPolicyClearKeyConfiguration(),
                        new ContentKeyPolicyTokenRestriction(Issuer, Audience, primaryKey,
                            ContentKeyPolicyRestrictionTokenType.Jwt, alternateKeys, requiredClaims))
                };

                policy = await client.ContentKeyPolicies.CreateOrUpdateAsync(resourceGroupName, accountName, contentKeyPolicyName, options);
            }
            
            return policy;
        }

        private static async Task CleanUpAsync(
           IAzureMediaServicesClient client,
           string resourceGroupName,
           string accountName,
           string transformName,
           string assetName,
           string locatorName,
           string jobName,
           string contentKeyPolicyName,
           bool stopEndpoint,
           string streamingEndpointName
           )
        {
            Console.WriteLine("Deleting Asset");
            await client.Assets.DeleteAsync(resourceGroupName, accountName, assetName);

            Console.WriteLine("Deleting Job");
            await client.Jobs.DeleteAsync(resourceGroupName, accountName, transformName, jobName);


            Console.WriteLine("Deleting Transform");
            await client.Transforms.DeleteAsync(resourceGroupName, accountName, transformName);

         
          
            if (stopEndpoint)
            {
                Console.WriteLine("Stopping the Streaming Endpoint");
                await client.StreamingEndpoints.StopAsync(resourceGroupName, accountName, streamingEndpointName);
            }

            Console.WriteLine("Cleanup Complete");
            
        }


        private static async Task CleanUpEverything(IAzureMediaServicesClient client, string resourceGroupName,
           string accountName)
        {
            var allAssets = await client.Assets.ListAsync(resourceGroupName, accountName);

            foreach(var asset in allAssets)
            {
                await client.Assets.DeleteAsync(resourceGroupName, accountName, asset.Name);
            }

            var allTransforms = await client.Transforms.ListAsync(resourceGroupName, accountName);

            foreach (var transform in allTransforms)
            {
                // first delete all jobs for the given transform
                var transformJobs = await client.Jobs.ListAsync(resourceGroupName, accountName,transform.Name);

                foreach(var transformJob in transformJobs)
                {
                    await client.Jobs.DeleteAsync(resourceGroupName, accountName, transform.Name, transformJob.Name);

                }

                await client.Transforms.DeleteAsync(resourceGroupName, accountName, transform.Name);
            }


            var allKeyPolicies = await client.ContentKeyPolicies.ListAsync(resourceGroupName, accountName);
            foreach(var policy in allKeyPolicies)
            {
                await client.ContentKeyPolicies.DeleteAsync(resourceGroupName, accountName, policy.Name);
            }

        }
    }
}
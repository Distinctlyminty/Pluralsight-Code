using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Service;
using Microsoft.Azure.Devices.Shared;

namespace Provisioning
{
    public class ProvisioningService
    {
        ProvisioningServiceClient _provisioningServiceClient;
        string _tpmEndorsementKey = "Add TPM endorsment key";

        string _registrationId = "GM_TPM_Device_3";
        string _optionalDeviceId = "GB_TPM_82739";
        ProvisioningStatus? _optionalProvisioningStatus = ProvisioningStatus.Enabled;

        private DeviceCapabilities _optionalEdgeCapabilityEnabled =
            new DeviceCapabilities { IotEdge = true };

        private DeviceCapabilities _optionalEdgeCapabilityDisabled =
            new DeviceCapabilities { IotEdge = false };

        public ProvisioningService(ProvisioningServiceClient provisioningServiceClient)
        {
            _provisioningServiceClient = provisioningServiceClient;
        }

        internal async Task RunAsync()
        {
            await QueryIndividualEnrollmentsAsync().ConfigureAwait(false);
            await CreateIndividualEnrollmentTpmAsync().ConfigureAwait(false);
            await UpdateIndividualEnrollmentAsync().ConfigureAwait(false);
            await DeleteIndividualEnrollmentAsync().ConfigureAwait(false);
        }

        private async Task QueryIndividualEnrollmentsAsync()
        {
            Console.WriteLine("\nList all individual enrollments");
            QuerySpecification querySpecification = new QuerySpecification("SELECT * FROM enrollments");
            using (Query query = _provisioningServiceClient.CreateIndividualEnrollmentQuery(querySpecification))
            {
                while (query.HasNext())
                {
                    Console.WriteLine("\nQuerying the enrollments.");
                    QueryResult queryResult = await query.NextAsync().ConfigureAwait(false);
                    Console.WriteLine(queryResult);
                }
            }
        }

        private async Task CreateIndividualEnrollmentTpmAsync()
        {
            Console.WriteLine("\nCreating a new individual enrollment.");
            Attestation attestation = new TpmAttestation(_tpmEndorsementKey);
            IndividualEnrollment individualEnrollment =
                    new IndividualEnrollment(
                            _registrationId,
                            attestation);

            // Optional parameters
            individualEnrollment.DeviceId = _optionalDeviceId;
            individualEnrollment.ProvisioningStatus = _optionalProvisioningStatus; 
            individualEnrollment.Capabilities = _optionalEdgeCapabilityEnabled;
            individualEnrollment.InitialTwinState = new TwinState(
                null,
                new TwinCollection()
                {
                    ["Brand"] = "Globomantics",
                    ["Model"] = "GM1-2",
                    ["Color"] = "Blue",
                });

            Console.WriteLine("\nAdding new individual enrollment...");

            IndividualEnrollment individualEnrollmentResult =
                await _provisioningServiceClient.CreateOrUpdateIndividualEnrollmentAsync(individualEnrollment).ConfigureAwait(false);

            Console.WriteLine(individualEnrollmentResult);
        }

        private async Task UpdateIndividualEnrollmentAsync()
        {
            var individualEnrollment = await GetIndividualEnrollmentInfoAsync().ConfigureAwait(false);
            individualEnrollment.InitialTwinState.DesiredProperties["Color"] = "Yellow";
            individualEnrollment.Capabilities = _optionalEdgeCapabilityDisabled;

            IndividualEnrollment individualEnrollmentResult =
                await _provisioningServiceClient.CreateOrUpdateIndividualEnrollmentAsync(individualEnrollment).ConfigureAwait(false);
            Console.WriteLine(individualEnrollmentResult);
        }

        public async Task<IndividualEnrollment> GetIndividualEnrollmentInfoAsync()
        {
            Console.WriteLine("\nGetting an individual Enrollment.");
            IndividualEnrollment getResult =
                await _provisioningServiceClient.GetIndividualEnrollmentAsync(_registrationId).ConfigureAwait(false);
            Console.WriteLine(getResult);

            return getResult;
        }

        private async Task DeleteIndividualEnrollmentAsync() {
            Console.WriteLine("\nDeleting an individual enrollment...");
            await _provisioningServiceClient.DeleteIndividualEnrollmentAsync(_registrationId).ConfigureAwait(false);
        }






    }
}

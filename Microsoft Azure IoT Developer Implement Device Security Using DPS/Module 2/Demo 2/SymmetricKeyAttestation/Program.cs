using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PS.Attestation
{
    /// <summary>
    /// Provision a device using individual enrollment and a symmetric key
    /// </summary>
    public static class Program
    {
        private static string _globalDeviceEndpoint = "global.azure-devices-provisioning.net";
        private static string _idScope = "0ne001F8C27";

        private static string _registrationId = "MyIoTDevice-1";

        private static string _primaryKey = "Cq5r9Js21ifoprh4CFVM51T5bKz1Bleu5IpnGaBAHPf9SwoLfHh1j0oS+p6gvSsGp/B4MCHo3ybW8CksnH9dxA==";
        private static string _secondaryKey = "WSJ48XRScm4b77H9mSZFhwsL16KAC8m5dd0RurfIwPC+WAOlsxYfppbg+SiDXfamCDIGs34lAaC5jBxfMTcXzw==";
        public static async Task Main(string[] args)
        {
           
            using (var security =
                new SecurityProviderSymmetricKey(_registrationId,_primaryKey, _secondaryKey))
            using (var transport = new ProvisioningTransportHandlerHttp())
            {
                ProvisioningDeviceClient provClient =
                    ProvisioningDeviceClient.Create(_globalDeviceEndpoint, _idScope, security, transport);

                Console.WriteLine($"Provision the device...");

                using (var deviceClient = await ProvisionDevice(provClient, security))
                {
                    Console.WriteLine("Device provisioned");
                    await deviceClient.OpenAsync().ConfigureAwait(false);

                    Console.WriteLine("Start sending device telemetry...");
                    for (int i = 0; i <= 10; i++)
                    {
                         await SendMessagesAsync(deviceClient);
                    }

                    await deviceClient.CloseAsync().ConfigureAwait(false);
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();


        }

        private static async Task<DeviceClient> ProvisionDevice(ProvisioningDeviceClient provisioningDeviceClient, SecurityProviderSymmetricKey security)
        {
            var result = await provisioningDeviceClient.RegisterAsync().ConfigureAwait(false);
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception($"DeviceRegistrationResult.Status is NOT 'Assigned'");
            }

            var auth = new DeviceAuthenticationWithRegistrySymmetricKey(
                result.DeviceId,
                security.GetPrimaryKey());

            return DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp);
        }

        private static async Task SendMessagesAsync(DeviceClient deviceClient)
        {
            int speed = 0;
            var rand = new Random();


            int currentSpeed = speed + rand.Next(1, 60);

            string messageBody = JsonSerializer.Serialize(
                new
                {
                    vehicleRegistration = "RJ69 XRT",
                    vehiclespeed = currentSpeed

                });
            using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            await deviceClient.SendEventAsync(message);
            Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");


        }
    }
}

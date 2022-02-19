using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using static TPM.Simulator.SecurityProdierTpmSimulator;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace TPM
{
    class Program
    {

        private static string _globalDeviceEndpoint = "global.azure-devices-provisioning.net";
        private static string _idScope = "Add ID Scope";

        // The registration Id from the individual enrollment. 
        private static string _registrationId = "Add Registration ID";

        public static async Task<int> Main(string[] args)
        {
            // Parse application parameters
            Parameters parameters = null;
            ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(parsedParams =>
                {
                    parameters = parsedParams;
                })
                .WithNotParsed(errors =>
                {
                    Environment.Exit(1);
                });

            if (parameters.GetTpmEndorsementKey)
            {
                Console.WriteLine("Starting TPM simulator...");
                SecurityProviderTpmSimulator.StartSimulatorProcess();

                using (var security = new SecurityProviderTpmSimulator(null))
                {
                    Console.WriteLine($"Your Endorsement Key is {Convert.ToBase64String(security.GetEndorsementKey())}");
                }

                // if using a physical device
                //using(var security = new SecurityProviderTpmHsm(null))
                //{
                //   Console.WriteLine($"Your Endorsement Key is {Convert.ToBase64String(security.GetEndorsementKey())}");
                //}
                SecurityProviderTpmSimulator.StopSimulatorProcess();

                return 0;
            }

            Console.WriteLine("Starting TPM simulator...");
            SecurityProviderTpmSimulator.StartSimulatorProcess();

            await RunSampleAsync();


            return 0;
        }

        public static async Task RunSampleAsync()
        {

            try
            {
                Console.WriteLine("Starting the TPM simulator...");
                SecurityProviderTpmSimulator.StartSimulatorProcess();


                Console.WriteLine($"Initializing the device provisioning client...");

                using (var security = new SecurityProviderTpmSimulator(_registrationId))
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
            finally
            {

                SecurityProviderTpmSimulator.StopSimulatorProcess();

            }

        }

        private static async Task<DeviceClient> ProvisionDevice(ProvisioningDeviceClient provisioningDeviceClient, SecurityProviderTpm security)
        {
            var result = await provisioningDeviceClient.RegisterAsync().ConfigureAwait(false);
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception($"DeviceRegistrationResult.Status is NOT 'Assigned'");
            }

            var auth = new DeviceAuthenticationWithTpm(result.DeviceId, security);
            Console.WriteLine($"Testing the provisioned device with IoT Hub...");
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
            using var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            await deviceClient.SendEventAsync(message);
            Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");


        }

    }


}

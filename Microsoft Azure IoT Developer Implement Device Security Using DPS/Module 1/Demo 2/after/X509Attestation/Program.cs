﻿using Microsoft.Azure.Devices.Client;
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
    /// Provision a device with DPS using a self singed X509 certificate
    /// </summary>
    public static class Program
    {
        private static string _globalDeviceEndpoint = "global.azure-devices-provisioning.net";
        private static string _idScope = "0ne001F8C27";
        private static string _certificateFileName = "certificate.pfx";
        private static string _certificatePassword = "1234";

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Loading the certificate...");

            var primaryPFX = LoadProvisioningCertificate();


            using (var security = new SecurityProviderX509Certificate(primaryPFX))
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

        private static async Task<DeviceClient> ProvisionDevice(ProvisioningDeviceClient provisioningDeviceClient, SecurityProviderX509Certificate security)
        {
            var result = await provisioningDeviceClient.RegisterAsync().ConfigureAwait(false);
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception($"DeviceRegistrationResult.Status is NOT 'Assigned'");
            }

            var auth = new DeviceAuthenticationWithX509Certificate(
                result.DeviceId,
                security.GetAuthenticationCertificate());

            return DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp);
        }

       

        private static X509Certificate2 LoadProvisioningCertificate()
        {
            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(_certificateFileName, _certificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;

            foreach (X509Certificate2 element in certificateCollection)
            {
                Console.WriteLine($"Found certificate: {element?.Thumbprint} {element?.Subject}; PrivateKey: {element?.HasPrivateKey}");
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                throw new FileNotFoundException($"{_certificateFileName} did not contain any certificate with a private key.");
            }

            Console.WriteLine($"Using certificate {certificate.Thumbprint} {certificate.Subject}");
            return certificate;
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

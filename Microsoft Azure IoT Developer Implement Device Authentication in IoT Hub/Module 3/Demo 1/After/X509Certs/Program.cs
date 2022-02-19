using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;

namespace X509Certs
{
    class Program
    {
        private static DeviceClient _deviceClient;


        static async Task Main(string[] args)
        {
            var primaryPFX = new X509Certificate2(@"Certificates/primary.pfx", "1234");
            var primaryAuth = new DeviceAuthenticationWithX509Certificate("SelfSignedDevice1", primaryPFX);

            var secondaryPFX = new X509Certificate2(@"Certificates/secondary.pfx", "1234");
            var secondaryAuth = new DeviceAuthenticationWithX509Certificate("SelfSignedDevice1", secondaryPFX);

            _deviceClient = DeviceClient.Create("pstest.azure-devices.net", primaryAuth, TransportType.Amqp_Tcp_Only);

      

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            try
            {
                await SendMessagesAsync(cts.Token);

            }
            catch (UnauthorizedException  ex)
            {
                // if primary authentication fails then try again with the secondary authentication thumbprint 
                _deviceClient = DeviceClient.Create("pstest.azure-devices.net", secondaryAuth, TransportType.Amqp_Tcp_Only);
                await SendMessagesAsync(cts.Token);
            }
      

            _deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");

        }

        private static async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            int speed = 0;
            var rand = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
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

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                await Task.Delay(1000);
            }
        }
    }
}

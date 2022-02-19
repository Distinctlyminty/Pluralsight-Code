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

﻿using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace SymmetricKeys
{
    class Program
    {
        private static string _primaryConnectionString = "HostName=iot-ps-eastus.azure-devices.net;DeviceId=ps-test-1;SharedAccessKey=eZjpjrUn0HtW/ao/4lsn4KDrMpMqTftRnoChHqeLVE8=";
        private static DeviceClient _deviceClient;

        static async Task Main(string[] args)
        {   
            _deviceClient = DeviceClient.CreateFromConnectionString(_primaryConnectionString,TransportType.Mqtt);

           
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendMessagesAsync(cts.Token);

            _deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");

        }

        private static async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            int speed = 0;
            var rand = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                int currentSpeed = speed + rand.Next(1,60);

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

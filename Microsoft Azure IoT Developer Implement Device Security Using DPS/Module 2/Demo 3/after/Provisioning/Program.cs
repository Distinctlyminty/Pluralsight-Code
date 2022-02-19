using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Service;

namespace Provisioning
{
    class Program
    {
        private static string _connectionString = "Add DPS Connection String";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            using (var provisioningServiceClient = ProvisioningServiceClient.CreateFromConnectionString(_connectionString))
            {
                var service = new ProvisioningService(provisioningServiceClient);
                service.RunAsync().GetAwaiter().GetResult();

                }

            Console.WriteLine("Done.\n");
        }
    }
}

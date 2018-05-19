using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading;

namespace console
{
    class Program
    {
        public static IConfigurationRoot Configuration {get; set;}
        
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            ReadQueue();
        }

        private static void ReadQueue(){
            // Configure a connection string for Azure Storage
            // https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string
            var queueConnection = Configuration["ConnectionStrings:QueueConnection"];

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(queueConnection);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("loadleveling");

            while(true){
                var message = queue.GetMessageAsync();

                message.Wait();

                if(message.Result != null){
                    Console.WriteLine(message.Result.AsString);

                    var deleteMessageTask = queue.DeleteMessageAsync(message.Result);

                    deleteMessageTask.Wait();
                }
                else{
                    Console.WriteLine("No message to display...");
                    Thread.Sleep(3000);
                }
            }
        }
    }
}

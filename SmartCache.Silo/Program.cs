using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

namespace SmartCache.Silo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder.UseAzureStorageClustering(configureOptions: options =>
                    {
                        options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
                    });

                    siloBuilder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "SmartCache";
                        options.ServiceId = "SmartCache";
                    });

                    siloBuilder.AddAzureBlobGrainStorage("blobStorage", configureOptions: options =>
                    {
                        options.BlobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true;");
                    });

                    siloBuilder.Configure<GrainCollectionOptions>(options =>
                    {
                        options.CollectionQuantum = TimeSpan.FromMinutes(1);
                        options.CollectionAge =  TimeSpan.FromMinutes(5);
                    });

                }).RunConsoleAsync();
        }
    }
}

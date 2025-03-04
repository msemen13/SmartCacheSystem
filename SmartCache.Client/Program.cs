using Orleans.Configuration;
using SmartCache.Grains.Abstractions;

namespace SmartCache.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseOrleansClient((context, client) =>
            {
                client.UseAzureStorageClustering(configureOptions: options =>
                {
                    options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
                });

                client.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "SmartCache";
                    options.ServiceId = "SmartCache";
                });
            });

            var app = builder.Build();

            app.MapGet("checkemail/{email}", async (string email, IClusterClient clusterClient) =>
            {
                var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);

                var status = await checkingEmailGrain.IsBreached();

                if (status)
                {
                    return Results.Ok();
                }
                else
                {
                    return Results.NotFound();
                }
            });

            app.MapPost("addemail", async (string email, IClusterClient clusterClient) =>
            {
                var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);

                var status = await checkingEmailGrain.AddBreachedEmail();

                if (status)
                {
                    return Results.Created();
                }
                else
                {
                    return Results.Conflict();
                }
            });

            app.MapPost("addemails", async (List<string> emails, IClusterClient clusterClient) =>
            {
                foreach (var email in emails)
                {
                    var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);

                    await checkingEmailGrain.SetBreachedStatus(true);
                }

                return Results.Ok();

            });

            app.MapPost("deleteemail", async (string email, IClusterClient clusterClient) =>
            {
                var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);

                await checkingEmailGrain.Remove();

                return Results.Ok();

            });

            app.Run();
        }
    }
}

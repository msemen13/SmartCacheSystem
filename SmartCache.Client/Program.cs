using Orleans.Configuration;
using SmartCache.Grains.Abstractions;
using System.Net.Mail;
using System.Text.RegularExpressions;

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
                //if (!IsValidEmail(email))
                //{
                //    return Results.BadRequest("Invalid email format.");
                //}

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
                if (emails == null || emails.Count == 0)
                {
                    return Results.BadRequest("Email list is empty.");
                }

                var invalidEmails = emails.Where(email => !IsValidEmail(email)).ToList();
                var validEmails = emails.Except(invalidEmails).ToList();


                var tasks = validEmails.Select(async email =>
                {
                    var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);
                    var status = await checkingEmailGrain.AddBreachedEmail();
                    return (email, status);
                });

                var results = await Task.WhenAll(tasks);

                var addedEmails = results.Where(x => x.status).Select(x => x.email).ToList();
                var breachedEmails = results.Where(x => !x.status).Select(x => x.email).ToList();


                //var addedEmails = new List<string>();
                //var breachedEmails = new List<string>();

                //foreach (var email in emails)
                //{
                //    if (!IsValidEmail(email))
                //    {
                //        invalidEmails.Add(email);
                //        continue;
                //    }

                //    var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);
                //    var status = await checkingEmailGrain.AddBreachedEmail();

                //    if (status)
                //    {
                //        addedEmails.Add(email);
                //    }
                //    else
                //    {
                //        breachedEmails.Add(email);
                //    }
                //}

                var response = new
                {
                    AddedEmails = addedEmails,
                    BreachedEmails = breachedEmails,
                    InvalidEmails = invalidEmails
                };

                return Results.Ok(response);

                //foreach (var email in emails)
                //{
                //    var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);

                //    await checkingEmailGrain.AddBreachedEmail();
                //}

                //return Results.Ok();

            });

            app.MapPost("deleteemail", async (string email, IClusterClient clusterClient) =>
            {
                var checkingEmailGrain = clusterClient.GetGrain<IBreachedEmailGrain>(email);

                await checkingEmailGrain.Remove();

                return Results.Ok();

            });

            app.Run();
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var mail = new MailAddress(email);

                string pattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

    }
}
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using SmartCache.Grains.Abstractions;

namespace SmartCache.Test
{
    public class TestFixture : IAsyncLifetime
    {
        public TestCluster Cluster { get; private set; }
        public IClusterClient ClusterClient => Cluster.Client;

        public TestFixture()
        {
            Cluster = new TestClusterBuilder()
                .AddSiloBuilderConfigurator<TestSiloConfigurator>()
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Cluster.DeployAsync();
        }

        public async Task DisposeAsync()
        {
            await Cluster.StopAllSilosAsync();
        }

        public class TestSiloConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder
                    .AddMemoryGrainStorage("blobStorage")
                    .ConfigureLogging(logging => logging.AddConsole());
            }
        }
    }
}
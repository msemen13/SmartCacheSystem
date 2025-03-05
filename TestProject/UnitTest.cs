using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Orleans;
using SmartCache.Grains.Abstractions;
using SmartCache.Test;
using Orleans.TestingHost;

namespace SmartCache.Tests
{
    public class BreachedEmailGrainTests : IClassFixture<TestFixture>
    {
        private readonly IClusterClient _clusterClient;

        public BreachedEmailGrainTests(TestFixture fixture)
        {
            _clusterClient = fixture.ClusterClient;
        }

        [Fact]
        public async Task Test_IsBreached_ReturnsFalse_Initially()
        {
            // Arrange
            var email = "test@gmail.com";
            var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);

            // Act
            var isBreached = await grain.IsBreached();

            // Assert
            isBreached.Should().BeFalse();
        }

        [Fact]
        public async Task Test_AddBreachedEmail_UpdateStatus()
        {
            // Arrange
            var email = "test2@gmail.com";
            var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);

            // Act
            var added = await grain.AddBreachedEmail();
            var isBreached = await grain.IsBreached();

            // Assert
            added.Should().BeTrue();
            isBreached.Should().BeTrue();
        }

        [Fact]
        public async Task Test_AddBreachedEmail_WhenAlreadyBreached_ReturnsFalse()
        {
            // Arrange
            var email = "test3@gmail.com";
            var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);

            // Act
            await grain.AddBreachedEmail();
            var addAgain = await grain.AddBreachedEmail();

            // Assert
            addAgain.Should().BeFalse();
        }

        [Fact]
        public async Task Test_RemoveEmail_ResetBreachedStatus()
        {
            // Arrange
            var email = "test4@gmail.com";
            var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);

            // Act
            await grain.AddBreachedEmail();
            await grain.Remove();
            var isBreached = await grain.IsBreached();

            // Assert
            isBreached.Should().BeFalse();
        }

        [Fact]
        public async Task Test_AddMultipleBreachedEmails_UpdateStatus()
        {
            // Arrange
            var email1 = "test5@gmail.com";
            var email2 = "test6@gmail.com";
            var email3 = "test7@gmail.com";

            var grain1 = _clusterClient.GetGrain<IBreachedEmailGrain>(email1);
            var grain2 = _clusterClient.GetGrain<IBreachedEmailGrain>(email2);
            var grain3 = _clusterClient.GetGrain<IBreachedEmailGrain>(email3);

            // Act
            await grain1.AddBreachedEmail();
            await grain2.AddBreachedEmail();
            await grain3.AddBreachedEmail();

            var isBreached1 = await grain1.IsBreached();
            var isBreached2 = await grain2.IsBreached();
            var isBreached3 = await grain3.IsBreached();

            // Assert
            isBreached1.Should().BeTrue();
            isBreached2.Should().BeTrue();
            isBreached3.Should().BeTrue();
        }

        [Fact]
        public async Task Test_RemoveMultipleEmails_ResetBreachedStatus()
        {
            // Arrange
            var email1 = "test8@gmail.com";
            var email2 = "test9@gmail.com";
            var email3 = "test10@gmail.com";

            var grain1 = _clusterClient.GetGrain<IBreachedEmailGrain>(email1);
            var grain2 = _clusterClient.GetGrain<IBreachedEmailGrain>(email2);
            var grain3 = _clusterClient.GetGrain<IBreachedEmailGrain>(email3);

            // Act
            await grain1.AddBreachedEmail();
            await grain2.AddBreachedEmail();
            await grain3.AddBreachedEmail();

            await grain1.Remove();
            await grain2.Remove();
            await grain3.Remove();

            // Assert
            (await grain1.IsBreached()).Should().BeFalse();
            (await grain2.IsBreached()).Should().BeFalse();
            (await grain3.IsBreached()).Should().BeFalse();
        }

        [Fact]
        public async Task Test_AddThenRemove_StatusChange()
        {
            // Arrange
            var email = "test11@gmail.com";
            var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);

            // Act
            await grain.AddBreachedEmail();
            var isBreachedAfterAdd = await grain.IsBreached();

            await grain.Remove();
            var isBreachedAfterRemove = await grain.IsBreached();

            // Assert
            isBreachedAfterAdd.Should().BeTrue();
            isBreachedAfterRemove.Should().BeFalse();
        }

        [Fact]
        public async Task Test_AddAndRemoveMultipleTimes_EmailStatusUpdate()
        {
            // Arrange
            var email = "test12@gmail.com";
            var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);

            // Act
            await grain.AddBreachedEmail();
            var firstStatus = await grain.IsBreached();
            await grain.Remove();
            var secondStatus = await grain.IsBreached();
            await grain.AddBreachedEmail();
            var thirdStatus = await grain.IsBreached();
            await grain.Remove();
            var fourthStatus = await grain.IsBreached();

            // Assert
            firstStatus.Should().BeTrue();
            secondStatus.Should().BeFalse();
            thirdStatus.Should().BeTrue();
            fourthStatus.Should().BeFalse();
        }

        [Fact]
        public async Task Test_Add1000Emails_ThenTryAddingAgain_ShouldReturnFalse()
        {
            // Arrange
            var emails = new List<string>();

            for (int i = 1001; i <= 2000; i++)
            {
                emails.Add($"test{i}@gmail.com");
            }

            // Act
            var addTasks = emails.Select(async email =>
            {
                var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);
                await grain.AddBreachedEmail();
                return await grain.IsBreached();
            });

            var addResults = await Task.WhenAll(addTasks);
            bool allBreached = addResults.All(status => status); 


            var conflictTasks = emails.Select(async email =>
            {
                var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);
                return await grain.AddBreachedEmail();
            });

            var conflictResults = await Task.WhenAll(conflictTasks);
            bool allAddedConflict = conflictResults.All(result => !result);

            // Assert
            allBreached.Should().BeTrue();
            allAddedConflict.Should().BeTrue();
        }

        [Fact]
        public async Task Test_AddAndRemove_1000Emails_UpdatesStatus()
        {
            // Arrange
            var emails = new List<string>();

            for (int i = 2001; i <= 3000; i++)
            {
                emails.Add($"test{i}@gmail.com");
            }

            // Act
            var addTasks = emails.Select(async email =>
            {
                var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);
                await grain.AddBreachedEmail();
                return await grain.IsBreached();
            });

            var addResults = await Task.WhenAll(addTasks);
            bool allBreached = addResults.All(status => status); 


            var removeTasks = emails.Select(async email =>
            {
                var grain = _clusterClient.GetGrain<IBreachedEmailGrain>(email);
                await grain.Remove();
                return await grain.IsBreached();
            });

            var removeResults = await Task.WhenAll(removeTasks);
            bool allRemoved = removeResults.All(status => !status); 

            // Assert
            allBreached.Should().BeTrue();
            allRemoved.Should().BeTrue();
        }
    }
}

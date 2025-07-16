using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.Shared.Abstractions.Azure;
using TotovBuilder.Shared.Azure;
using TotovBuilder.Shared.Extensions;
using Xunit;

namespace TotovBuilder.Shared.Test.Extensions
{
    /// <summary>
    /// Represents tests on the <see cref="ServiceCollectionExtensions"/> class.
    /// </summary>
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public async Task AddAzureBlobStorageManager_ShouldAddAzureBlobStorageManagerToServiceCollection()
        {
            // Arrange
            Mock<ILogger<AzureBlobStorageManager>> loggerMock = new();

            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(loggerMock.Object);

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction(IServiceProvider ServiceProvider)
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(string.Empty, 10);
            }

            // Act
            serviceCollection.AddAzureBlobStorageManager(getOptionsFunction);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IAzureBlobStorageManager service = serviceProvider.GetRequiredService<IAzureBlobStorageManager>();
            Result<string> result = await service.FetchBlob("Container", "Blob");

            // Assert
            (service is AzureBlobStorageManager).Should().BeTrue();
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@"Invalid configuration for connecting to Azure blob storage :
- Connection string : """",
- Execution timeout : 10,
- Container name : ""Container""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }
    }
}

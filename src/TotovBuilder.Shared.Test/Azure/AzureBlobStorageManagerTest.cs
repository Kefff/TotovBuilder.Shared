using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.Shared.Azure;
using Xunit;

namespace TotovBuilder.Shared.Test.Azure
{
    /// <summary>
    /// Represents tests on the <see cref="AzureBlobStorageManager"/> class.
    /// </summary>
    public class AzureBlobStorageManagerTest
    {
        [Theory]
        [InlineData("", "Container", 10)]
        [InlineData("ConnectionString", "", 10)]
        [InlineData("ConnectionString", "Container", 0)]
        public async Task FetchBlob_ShouldCheckConfiguration(string connectionString, string containerName, int executionTimeOut)
        {
            // Arrange
            Mock<ILogger<AzureBlobStorageManager>> loggerMock = new Mock<ILogger<AzureBlobStorageManager>>();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeOut);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(loggerMock.Object, getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.FetchBlob(containerName, "Blob");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@$"Invalid configuration for connecting to Azure blob storage :
- Connection string : ""{connectionString}"",
- Execution timeout : {executionTimeOut},
- Container name : ""{containerName}""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "Container", 10)]
        [InlineData("ConnectionString", "", 10)]
        [InlineData("ConnectionString", "Container", 0)]
        public async Task UpdateContainer_ShouldCheckConfiguration(string connectionString, string containerName, int executionTimeOut)
        {
            // Arrange
            Mock<ILogger<AzureBlobStorageManager>> loggerMock = new Mock<ILogger<AzureBlobStorageManager>>();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeOut);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(loggerMock.Object, getOptionsFunction);

            // Act
            Result result = await azureBlobStorageManager.UpdateContainer(containerName, new Dictionary<string, byte[]>());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@$"Invalid configuration for connecting to Azure blob storage :
- Connection string : ""{connectionString}"",
- Execution timeout : {executionTimeOut},
- Container name : ""{containerName}""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "Container", 10)]
        [InlineData("ConnectionString", "", 10)]
        [InlineData("ConnectionString", "Container", 0)]
        public async Task UpdateBlob_ShouldCheckConfiguration(string connectionString, string containerName, int executionTimeOut)
        {
            // Arrange
            Mock<ILogger<AzureBlobStorageManager>> loggerMock = new Mock<ILogger<AzureBlobStorageManager>>();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeOut);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(loggerMock.Object, getOptionsFunction);

            // Act
            Result result = await azureBlobStorageManager.UpdateBlob(containerName, "Blob", Array.Empty<byte>());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@$"Invalid configuration for connecting to Azure blob storage :
- Connection string : ""{connectionString}"",
- Execution timeout : {executionTimeOut},
- Container name : ""{containerName}""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }
    }
}

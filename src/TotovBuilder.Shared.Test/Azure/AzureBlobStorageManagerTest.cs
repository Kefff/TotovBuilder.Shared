using System.Text;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.Shared.Abstractions.Azure;
using TotovBuilder.Shared.Azure;
using Xunit;

namespace TotovBuilder.Shared.Test.Azure
{
    /// <summary>
    /// Represents tests on the <see cref="AzureBlobStorageManager"/> class.
    /// </summary>
    public class AzureBlobStorageManagerTest
    {
        [Fact]
        public async Task FetchBlob_ShouldFetchBlob()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            string blobName = "index.html";
            string data = "<html><body><p>Hello World!</p></body></html>";

            Mock<IBlockBlobClientWrapper> blockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            blockBlobClientMock
                .Setup(m => m.DownloadToAsync(It.IsAny<Stream>()))
                .Callback((Stream s) => s.Write(Encoding.UTF8.GetBytes(data)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient(blobName)).Returns(blockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.FetchBlob(containerName, blobName);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(data);
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            blockBlobClientMock.Verify();
        }

        [Theory]
        [InlineData("", "Container", 10)]
        [InlineData("ConnectionString", "", 10)]
        [InlineData("ConnectionString", "Container", 0)]
        public async Task FetchBlob_WithInvalidConfiguration_ShouldFail(string connectionString, string containerName, int executionTimeout)
        {
            // Arrange
            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                new Mock<IBlobContainerClientWrapperFactory>().Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.FetchBlob(containerName, "Blob");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@$"Invalid configuration for connecting to Azure blob storage :
- Connection string : ""{connectionString}"",
- Execution timeout : {executionTimeout},
- Container name : ""{containerName}""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }

        [Fact]
        public async Task FetchBlob_WithTimedout_ShouldFail()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 1;
            string blobName = "index.html";

            Mock<IBlockBlobClientWrapper> blockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            blockBlobClientMock
                .Setup(m => m.DownloadToAsync(It.IsAny<Stream>()))
                .Returns(Task.Delay(1500))
                .Verifiable();

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient(blobName)).Returns(blockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.FetchBlob(containerName, blobName);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be("Execution delay (1s) exceeded while fetching Azure blob \"index.html\" from container \"$web\".");
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            blockBlobClientMock.Verify();
        }

        [Fact]
        public async Task FetchBlob_WithException_ShouldFail()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            string blobName = "index.html";

            Mock<IBlockBlobClientWrapper> blockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            blockBlobClientMock
                .Setup(m => m.DownloadToAsync(It.IsAny<Stream>()))
                .Throws(new Exception("Download error"))
                .Verifiable();

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient(blobName)).Returns(blockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.FetchBlob(containerName, blobName);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().StartWith(@"Error while fetching Azure blob ""index.html"" from container ""$web"" :
System.Exception: Download error");
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            blockBlobClientMock.Verify();
        }

        [Theory]
        [InlineData(true, "index.html", false)]
        [InlineData(false, "index.html", false)]
        [InlineData(false, "index", true)]
        public async Task UpdateBlob_ShouldUpdateBlob(bool useBytes, string blobName, bool alreadyHasHttpHeaders)
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            string data = "<html><body><p>Hello World!</p></body></html>";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            BlobHttpHeaders? httpHeaders = alreadyHasHttpHeaders
                ? new BlobHttpHeaders() { CacheControl = "max-age=3600" }
                : null;

            Mock<IBlockBlobClientWrapper> blockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            blockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.CreateIfNotExists()).Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient(blobName)).Returns(blockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result;

            if (useBytes)
            {
                result = await azureBlobStorageManager.UpdateBlob(containerName, blobName, bytes, httpHeaders);
            }
            else
            {
                result = await azureBlobStorageManager.UpdateBlob(containerName, blobName, data, httpHeaders);
            }

            // Assert
            result.IsSuccess.Should().BeTrue();
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            blockBlobClientMock.Verify();
        }

        [Fact]
        public async Task UpdateBlob_WithTimeout_ShouldFail()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 1;
            string blobName = "index.html";
            string data = "<html><body><p>Hello World!</p></body></html>";

            Mock<IBlockBlobClientWrapper> blockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            blockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.Delay(1500))
                .Verifiable();

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient(blobName)).Returns(blockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.UpdateBlob(containerName, blobName, data);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be("Execution delay (1s) exceeded while updating Azure blob \"index.html\" in container \"$web\".");
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            blockBlobClientMock.Verify();
        }

        [Fact]
        public async Task UpdateBlob_WithException_ShouldFail()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            string blobName = "index.html";
            string data = "<html><body><p>Hello World!</p></body></html>";

            Mock<IBlockBlobClientWrapper> blockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            blockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Throws(new Exception("Upload error"))
                .Verifiable();

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient(blobName)).Returns(blockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.UpdateBlob(containerName, blobName, data);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().StartWith(@"Error while updating Azure blob ""index.html"" in container ""$web"" :
System.Exception: Upload error");
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            blockBlobClientMock.Verify();
        }

        [Theory]
        [InlineData("", "Container", 10)]
        [InlineData("ConnectionString", "", 10)]
        [InlineData("ConnectionString", "Container", 0)]
        public async Task UpdateBlob_WithInvalidConfiguration_ShouldFail(string connectionString, string containerName, int executionTimeout)
        {
            // Arrange
            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                new Mock<IBlobContainerClientWrapperFactory>().Object,
                getOptionsFunction);

            // Act
            Result result = await azureBlobStorageManager.UpdateBlob(containerName, "Blob", Array.Empty<byte>(), It.IsAny<BlobHttpHeaders>());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@$"Invalid configuration for connecting to Azure blob storage :
- Connection string : ""{connectionString}"",
- Execution timeout : {executionTimeout},
- Container name : ""{containerName}""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public async Task UpdateContainer_ShouldUploadAndDeleteBlobsInContainer(bool useBytes, bool alreadyHasHttpHeaders)
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "index.html", "<html><body><p>Hello World!</p></body></html>" },
                { Path.Combine("src\\index.js"), "function start() { }" }
            };
            Dictionary<string, byte[]> bytes = new Dictionary<string, byte[]>();

            foreach (string key in data.Keys)
            {
                bytes.Add(key, Encoding.UTF8.GetBytes(data[key]));
            }

            Func<BlobHttpHeaders>? createHttpHeadersFunction = alreadyHasHttpHeaders
                ? () => new BlobHttpHeaders() { CacheControl = "max-age=3600" }
                : null;

            Mock<IBlockBlobClientWrapper> indexBlockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            indexBlockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock<IBlockBlobClientWrapper> javascriptBlockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            indexBlockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock<IBlockBlobClientWrapper> invalidBlockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            invalidBlockBlobClientMock.Setup(m => m.DeleteIfExists()).Verifiable();

            Mock<IBlobItemWrapper> indexBlobItemMock = new Mock<IBlobItemWrapper>();
            indexBlobItemMock.SetupGet(m => m.Name).Returns("index.html");

            Mock<IBlobItemWrapper> presetsBlobItemMock = new Mock<IBlobItemWrapper>();
            presetsBlobItemMock.SetupGet(m => m.Name).Returns("data/presets.json");

            Mock<IBlobItemWrapper> invalidBlobItemMock = new Mock<IBlobItemWrapper>();
            invalidBlobItemMock.SetupGet(m => m.Name).Returns("img/invalid.css");

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.CreateIfNotExists()).Verifiable();
            blobContainerClientMock
                .Setup(m => m.GetBlobs())
                .Returns(new IBlobItemWrapper[]
                {
                    indexBlobItemMock.Object,
                    presetsBlobItemMock.Object,
                    invalidBlobItemMock.Object
                })
                .Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient("index.html")).Returns(indexBlockBlobClientMock.Object).Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient("src/index.js")).Returns(javascriptBlockBlobClientMock.Object).Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient("img/invalid.css")).Returns(invalidBlockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result;

            if (useBytes)
            {
                result = await azureBlobStorageManager.UpdateContainer(containerName, bytes, createHttpHeadersFunction, "data/.*");
            }
            else
            {
                result = await azureBlobStorageManager.UpdateContainer(containerName, data, createHttpHeadersFunction, "data/.*");
            }

            // Assert
            result.IsSuccess.Should().BeTrue();
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            indexBlockBlobClientMock.Verify();
            javascriptBlockBlobClientMock.Verify();
            invalidBlockBlobClientMock.Verify();

        }

        [Fact]
        public async Task UpdateContainer_WithFailedUpdateTask_ShouldFail()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "index.html", "<html><body><p>Hello World!</p></body></html>" },
                { "src\\index.js", "function start() { }" }
            };

            Mock<IBlockBlobClientWrapper> indexBlockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            indexBlockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Throws(new Exception("Upload error"))
                .Verifiable();

            Mock<IBlockBlobClientWrapper> javascriptBlockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            javascriptBlockBlobClientMock
                .Setup(m => m.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>()))
                .Throws(new Exception("Connection error"))
                .Verifiable();

            Mock<IBlockBlobClientWrapper> invalidBlockBlobClientMock = new Mock<IBlockBlobClientWrapper>();
            invalidBlockBlobClientMock
                .Setup(m => m.DeleteIfExists());

            Mock<IBlobItemWrapper> indexBlobItemMock = new Mock<IBlobItemWrapper>();
            indexBlobItemMock.SetupGet(m => m.Name).Returns("index.html");

            Mock<IBlobItemWrapper> javascriptBlobItemMock = new Mock<IBlobItemWrapper>();
            javascriptBlobItemMock.SetupGet(m => m.Name).Returns("src/index.js");

            Mock<IBlobItemWrapper> invalidBlobItemMock = new Mock<IBlobItemWrapper>();
            invalidBlobItemMock.SetupGet(m => m.Name).Returns("img/invalid.css");

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.CreateIfNotExists()).Verifiable();
            blobContainerClientMock
                .Setup(m => m.GetBlobs())
                .Returns(new IBlobItemWrapper[]
                {
                    indexBlobItemMock.Object,
                    javascriptBlobItemMock.Object,
                    invalidBlobItemMock.Object
                })
                .Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient("index.html")).Returns(indexBlockBlobClientMock.Object).Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient("src/index.js")).Returns(javascriptBlockBlobClientMock.Object).Verifiable();
            blobContainerClientMock.Setup(m => m.GetBlockBlobClient("img/invalid.css")).Returns(invalidBlockBlobClientMock.Object).Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.UpdateContainer(containerName, data);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().StartWith(@"Error while updating Azure blob ""index.html"" in container ""$web"" :
System.Exception: Upload error");
            result.Errors[1].Message.Should().StartWith(@"Error while updating Azure blob ""src/index.js"" in container ""$web"" :
System.Exception: Connection error");
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
            indexBlockBlobClientMock.Verify();
            invalidBlockBlobClientMock.Verify();
        }

        [Fact]
        public async Task UpdateContainer_WithException_ShouldFail()
        {
            // Arrange
            string connectionString = "ConnectionString";
            string containerName = "$web";
            int executionTimeout = 10;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "index.html", "<html><body><p>Hello World!</p></body></html>" },
                { "src\\index.js", "function start() { }" }
            };

            Mock<IBlobContainerClientWrapper> blobContainerClientMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientMock.Setup(m => m.CreateIfNotExists()).Verifiable();
            blobContainerClientMock
                .Setup(m => m.GetBlobs())
                .Throws(new Exception("Connection error"))
                .Verifiable();

            Mock<IBlobContainerClientWrapperFactory> blobContainerClientWrapperFactoryMock = new Mock<IBlobContainerClientWrapperFactory>();
            blobContainerClientWrapperFactoryMock.Setup(m => m.Create(connectionString, containerName)).Returns(blobContainerClientMock.Object).Verifiable();

            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                blobContainerClientWrapperFactoryMock.Object,
                getOptionsFunction);

            // Act
            Result<string> result = await azureBlobStorageManager.UpdateContainer(containerName, data);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().StartWith(@"Error while updating Azure container ""$web"" :
System.Exception: Connection error");
            isGetOptionsFunctionCalled.Should().BeTrue();
            blobContainerClientWrapperFactoryMock.Verify();
            blobContainerClientMock.Verify();
        }

        [Theory]
        [InlineData("", "Container", 10)]
        [InlineData("ConnectionString", "", 10)]
        [InlineData("ConnectionString", "Container", 0)]
        public async Task UpdateContainer_WithInvalidConfiguration_ShouldFail(string connectionString, string containerName, int executionTimeout)
        {
            // Arrange
            bool isGetOptionsFunctionCalled = false;
            AzureBlobStorageManagerOptions getOptionsFunction()
            {
                isGetOptionsFunctionCalled = true;
                return new AzureBlobStorageManagerOptions(connectionString, executionTimeout);
            }

            AzureBlobStorageManager azureBlobStorageManager = new AzureBlobStorageManager(
                new Mock<ILogger<AzureBlobStorageManager>>().Object,
                new Mock<IBlobContainerClientWrapperFactory>().Object,
                getOptionsFunction);

            // Act
            Result result = await azureBlobStorageManager.UpdateContainer(containerName, new Dictionary<string, byte[]>(), It.IsAny<Func<BlobHttpHeaders>?>());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be(@$"Invalid configuration for connecting to Azure blob storage :
- Connection string : ""{connectionString}"",
- Execution timeout : {executionTimeout},
- Container name : ""{containerName}""");
            isGetOptionsFunctionCalled.Should().BeTrue();
        }
    }
}

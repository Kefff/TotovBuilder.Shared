using FluentAssertions;
using TotovBuilder.Shared.Azure;
using Xunit;

namespace TotovBuilder.Shared.Test.Azure
{
    /// <summary>
    /// Represents tests on the <see cref="AzureBlobStorageManagerOptions"/> class.
    /// </summary>
    public class AzureBlobStorageManagerOptionsTest
    {
        [Fact]
        public void Constructor_ShouldCreateNewInstance()
        {
            // Act
            AzureBlobStorageManagerOptions options = new AzureBlobStorageManagerOptions("$web", 30);

            // Assert
            options.ConnectionString.Should().Be("$web");
            options.ExecutionTimeout.Should().Be(30);
        }
    }
}

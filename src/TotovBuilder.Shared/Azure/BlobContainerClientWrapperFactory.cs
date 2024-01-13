using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Shared.Azure
{
    /// <summary>
    /// Represents an <see cref="IBlobContainerClientWrapper"/> factory.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the BlobContainerClient class.")]
    public class BlobContainerClientWrapperFactory : IBlobContainerClientWrapperFactory
    {
        /// <inheritdoc/>
        public IBlobContainerClientWrapper Create(string connectionString, string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, blobContainerName);
            BlobContainerClientWrapper wrapper = new BlobContainerClientWrapper(blobContainerClient);

            return wrapper;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Shared.Azure
{
    /// <summary>
    /// Represents a <see cref="BlobContainerClient"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the BlockBlobClient class.")]
    public class BlockBlobClientWrapper : IBlockBlobClientWrapper
    {
        /// <summary>
        /// Instance.
        /// </summary>
        private BlockBlobClient Instance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockBlobClientWrapper"/> class.
        /// </summary>
        /// <param name="instance"><see cref="BlockBlobClient"/> instance.</param>
        public BlockBlobClientWrapper(BlockBlobClient instance)
        {
            Instance = instance;
        }

        /// <inheritdoc/>
        public void DeleteIfExists()
        {
            Instance.DeleteIfExists();
        }

        /// <inheritdoc/>
        public Task DownloadToAsync(Stream destination)
        {
            return Instance.DownloadToAsync(destination);
        }

        /// <inheritdoc/>
        public Task UploadAsync(Stream content, BlobHttpHeaders httpHeaders)
        {
            return Instance.UploadAsync(content, httpHeaders);
        }
    }
}

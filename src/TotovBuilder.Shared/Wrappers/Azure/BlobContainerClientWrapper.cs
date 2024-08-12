using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using TotovBuilder.Deployer.Abstractions.Wrappers.Azure;

namespace TotovBuilder.Shared.Wrappers.Azure
{
    /// <summary>
    /// Represents a <see cref="BlobContainerClient"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the BlobContainerClient class.")]
    public class BlobContainerClientWrapper : IBlobContainerClientWrapper
    {
        /// <summary>
        /// Instance.
        /// </summary>
        private BlobContainerClient Instance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClientWrapper"/> class.
        /// </summary>
        /// <param name="instance">Instance.</param>
        public BlobContainerClientWrapper(BlobContainerClient instance)
        {
            Instance = instance;
        }

        /// <inheritdoc/>
        public void CreateIfNotExists()
        {
            Instance.CreateIfNotExists();
        }

        /// <inheritdoc/>
        public IEnumerable<IBlobItemWrapper> GetBlobs()
        {
            List<BlobItemWrapper> wrappers = [];

            foreach (Page<BlobItem> page in Instance.GetBlobs().AsPages())
            {
                foreach (BlobItem existingBlob in page.Values)
                {
                    wrappers.Add(new BlobItemWrapper(existingBlob));
                }
            }

            return wrappers;
        }

        /// <inheritdoc/>
        public IBlockBlobClientWrapper GetBlockBlobClient(string blobName)
        {
            BlockBlobClient blockBlobClient = Instance.GetBlockBlobClient(blobName);
            BlockBlobClientWrapper wrapper = new(blockBlobClient);

            return wrapper;
        }
    }
}

using Azure;
using Azure.Storage.Blobs;

namespace TotovBuilder.Shared.Abstractions.Wrappers.Azure
{
    /// <summary>
    /// Provides the functionalities of a <see cref="BlobContainerClient"/> wrapper.
    /// </summary>
    public interface IBlobContainerClientWrapper
    {
        /// <summary>
        /// The <see cref="Create(PublicAccessType, Metadata, BlobContainerEncryptionScopeOptions, CancellationToken)"/>
        /// operation creates a new container
        /// under the specified account. If the container with the same name
        /// already exists, the operation fails.
        ///
        /// For more information, see
        /// <see href="https://docs.microsoft.com/rest/api/storageservices/create-container">
        /// Create Container</see>.
        /// </summary>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        void Create();

        /// <summary>
        /// The <see cref="Exists"/> operation can be called on a
        /// <see cref="BlobContainerClient"/> to see if the associated container
        /// exists on the storage account in the storage service.
        /// </summary>
        /// <returns>
        /// Returns true if the container exists.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs. If you want to create the container if
        /// it doesn't exist, use
        /// <see cref="CreateIfNotExists(PublicAccessType, Metadata, BlobContainerEncryptionScopeOptions, CancellationToken)"/>
        /// instead.
        /// </remarks>
        bool Exists();

        /// <summary>
        /// The <see cref="GetBlobs"/> operation returns an async sequence
        /// of blobs in this container.  Enumerating the blobs may make
        /// multiple requests to the service while fetching all the values.
        /// Blobs are ordered lexicographically by name.
        ///
        /// For more information, see
        /// <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs">
        /// List Blobs</see>.
        /// </summary>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// An <see cref="Pageable{T}"/> of <see cref="BlobItem"/>
        /// describing the blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        IEnumerable<IBlobItemWrapper> GetBlobs();

        /// <summary>
        /// Create a new <see cref="BlobClient"/> object by appending
        /// <paramref name="blobName"/> to the end of <see cref="Uri"/>.  The
        /// new <see cref="BlobClient"/> uses the same request policy
        /// pipeline as the <see cref="BlobContainerClient"/>.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <returns>A new <see cref="BlobClient"/> instance.</returns>
        IBlockBlobClientWrapper GetBlockBlobClient(string blobName);
    }
}

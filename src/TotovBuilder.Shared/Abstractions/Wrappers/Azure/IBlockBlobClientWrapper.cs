using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TotovBuilder.Deployer.Abstractions.Wrappers.Azure
{
    /// <summary>
    /// Provides the functionalities of a <see cref="BlobContainerClient"/> wrapper.
    /// </summary>
    public interface IBlockBlobClientWrapper
    {
        /// <summary>
        /// The <see cref="DeleteIfExists"/> operation marks the specified blob
        /// or snapshot for deletion, if the blob exists. The blob is later deleted
        /// during garbage collection which could take several minutes.
        ///
        /// Note that in order to delete a blob, you must delete all of its
        /// snapshots. You can delete both at the same time using
        /// <see cref="DeleteSnapshotsOption.IncludeSnapshots"/>.
        ///
        /// For more information, see
        /// <see href="https://docs.microsoft.com/rest/api/storageservices/delete-blob">
        /// Delete Blob</see>.
        /// </summary>
        /// <param name="snapshotsOption">
        /// Specifies options for deleting blob snapshots.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add conditions on
        /// deleting this blob.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> Returns true if blob exists and was
        /// marked for deletion, return false otherwise.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        void DeleteIfExists();

        /// <summary>
        /// The <see cref="DownloadToAsync(Stream)"/> downloads a blob using parallel requests,
        /// and writes the content to <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">
        /// A <see cref="Stream"/> to write the downloaded content to.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> describing the operation.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        Task DownloadToAsync(Stream destination);

        /// <summary>
        /// The <see cref="UploadAsync(Stream, BlobHttpHeaders, Metadata, BlobRequestConditions, AccessTier?, IProgress{long}, CancellationToken)"/>
        /// operation overwrites the contents of the blob, creating a new block
        /// blob if none exists.  Overwriting an existing block blob replaces
        /// any existing metadata on the blob.
        ///
        /// Set <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/specifying-conditional-headers-for-blob-service-operations">
        /// access conditions</see> through <see cref="BlobRequestConditions"/>
        /// to avoid overwriting existing data.
        ///
        /// Partial updates are not supported with <see cref="UploadAsync(Stream, BlobHttpHeaders, Metadata, BlobRequestConditions, AccessTier?, IProgress{long}, CancellationToken)"/>;
        /// the content of the existing blob is overwritten with the content
        /// of the new blob.  To perform a partial update of the content of a
        /// block blob, use the <see cref="StageBlockAsync(string, Stream,  byte[], BlobRequestConditions, IProgress{long}, CancellationToken)"/> and
        /// <see cref="CommitBlockListAsync(IEnumerable{string}, BlobHttpHeaders, Metadata, BlobRequestConditions, AccessTier?, CancellationToken)" /> operations.
        ///
        /// For more information, see
        /// <see href="https://docs.microsoft.com/rest/api/storageservices/put-blob">
        /// Put Blob</see>.
        /// </summary>
        /// <param name="content">
        /// A <see cref="Stream"/> containing the content to upload.
        /// </param>
        /// <param name="httpHeaders">
        /// Optional standard HTTP header properties that can be set for the
        /// block blob.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this block blob.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the creation of this new block blob.
        /// </param>
        /// <param name="accessTier">
        /// Optional <see cref="AccessTier"/>
        /// Indicates the tier to be set on the blob.
        /// </param>
        /// <param name="progressHandler">
        /// Optional <see cref="IProgress{Long}"/> to provide
        /// progress updates about data transfers.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContentInfo}"/> describing the
        /// state of the updated block blob.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        Task UploadAsync(Stream content, BlobHttpHeaders httpHeaders);
    }
}

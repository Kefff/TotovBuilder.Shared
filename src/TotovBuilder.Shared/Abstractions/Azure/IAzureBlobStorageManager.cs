using FluentResults;

namespace TotovBuilder.Shared.Abstractions.Azure
{
    /// <summary>
    /// Provides the functionalities of an Azure blob storage manager.
    /// </summary>
    public interface IAzureBlobStorageManager
    {
        /// <summary>
        /// Fetches data from an Azure blob.
        /// </summary>
        /// <param name="containerName">Name of the container containing the blob to fetch.</param>
        /// <param name="blobName">Name of the blob to fetch.</param>
        /// <returns>Blob data.</returns>
        Task<Result<string>> FetchBlob(string containerName, string blobName);

        /// <summary>
        /// Updates the whole content of a Azure blob container.
        /// New blobs are create, and existing blobs are update or deleted.
        /// </summary>
        /// <param name="containerName">Name of the container to update.</param>
        /// <param name="data">List of blob names and their data.</param>
        /// <param name="deletionIgnorePatterns">Patterns to avoid deleting matching blobs.</param>
        Task<Result> UpdateContainer(string containerName, Dictionary<string, string> data, params string[] deletionIgnorePatterns);

        /// <summary>
        /// Updates the whole content of a Azure blob container.
        /// New blobs are create, and existing blobs are update or deleted.
        /// </summary>
        /// <param name="containerName">Name of the container to update.</param>
        /// <param name="data">List of blob names and their data.</param>
        /// <param name="deletionIgnorePatterns">Patterns to avoid deleting matching blobs.</param>
        Task<Result> UpdateContainer(string containerName, Dictionary<string, byte[]> data, params string[] deletionIgnorePatterns);

        /// <summary>
        /// Updates data of an Azure blob.
        /// </summary>
        /// <param name="containerName">Name of the container containing the blob to update.</param>
        /// <param name="blobName">Name of the blob to update.</param>
        /// <param name="data">Data to upload.</param>
        Task<Result> UpdateBlob(string containerName, string blobName, string data);

        /// <summary>
        /// Updates data of an Azure blob.
        /// </summary>
        /// <param name="containerName">Name of the container containing the blob to update.</param>
        /// <param name="blobName">Name of the blob to update.</param>
        /// <param name="data">Data to upload.</param>
        Task<Result> UpdateBlob(string containerName, string blobName, byte[] data);
    }
}

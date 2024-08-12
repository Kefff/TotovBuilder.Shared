using System.Text;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using MimeMapping;
using TotovBuilder.Deployer.Abstractions.Wrappers.Azure;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Shared.Azure
{
    /// <summary>
    /// Represents an Azure blob storage manager.
    /// </summary>
    public class AzureBlobStorageManager : IAzureBlobStorageManager
    {
        /// <summary>
        /// Options.
        /// </summary>
        private AzureBlobStorageManagerOptions Options
        {
            get
            {
                if (_options == null)
                {
                    _options = _getOptionsFunction();
                }

                return _options;
            }
        }
        private AzureBlobStorageManagerOptions? _options = null;
        private readonly Func<AzureBlobStorageManagerOptions> _getOptionsFunction;

        /// <summary>
        /// Blob container client wrapper factory.
        /// </summary>
        private readonly IBlobContainerClientWrapperFactory BlobContainerClientWrapperFactory;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AzureBlobStorageManager> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageManager"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobContainerClientWrapperFactory">Blob container client wrapper factory.</param>
        /// <param name="getOptionsFunction">Function for getting the options to use.</param>
        public AzureBlobStorageManager(ILogger<AzureBlobStorageManager> logger, IBlobContainerClientWrapperFactory blobContainerClientWrapperFactory, Func<AzureBlobStorageManagerOptions> getOptionsFunction)
        {
            _getOptionsFunction = getOptionsFunction;
            BlobContainerClientWrapperFactory = blobContainerClientWrapperFactory;
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task<Result<string>> FetchBlob(string containerName, string blobName)
        {
            Result configurationCheckResult = CheckConfiguration(containerName);

            if (!configurationCheckResult.IsSuccess)
            {
                return Task.FromResult<Result<string>>(configurationCheckResult);
            }

            return Task.Run(() => ExecuteFetchBlob(containerName, blobName));
        }

        /// <inheritdoc/>
        public Task<Result> UpdateBlob(string containerName, string blobName, string data, BlobHttpHeaders? httpHeaders = null)
        {
            byte[] encodedData = Encoding.UTF8.GetBytes(data);

            return UpdateBlob(containerName, blobName, encodedData, httpHeaders);
        }

        /// <inheritdoc/>
        public Task<Result> UpdateBlob(string containerName, string blobName, byte[] data, BlobHttpHeaders? httpHeaders = null)
        {
            Result configurationCheckResult = CheckConfiguration(containerName);

            if (!configurationCheckResult.IsSuccess)
            {
                return Task.FromResult(configurationCheckResult);
            }

            return Task.Run(() => ExecuteCreateOrUpdateBlob(containerName, blobName, data, httpHeaders));
        }

        /// <inheritdoc/>
        public Task<Result> UpdateContainer(string containerName, Dictionary<string, string> data, Func<BlobHttpHeaders>? createHttpHeadersFunction = null, params string[] deletionIgnorePatterns)
        {
            Dictionary<string, byte[]> encodedData = data.ToDictionary(kvp => kvp.Key, kvp => Encoding.UTF8.GetBytes(kvp.Value));

            return UpdateContainer(containerName, encodedData, createHttpHeadersFunction, deletionIgnorePatterns);
        }

        /// <inheritdoc/>
        public Task<Result> UpdateContainer(string containerName, Dictionary<string, byte[]> data, Func<BlobHttpHeaders>? createHttpHeadersFunction = null, params string[] deletionIgnorePatterns)
        {
            Result configurationCheckResult = CheckConfiguration(containerName);

            if (!configurationCheckResult.IsSuccess)
            {
                return Task.FromResult(configurationCheckResult);
            }

            return Task.Run(() => ExecuteUpdateContainer(containerName, data, createHttpHeadersFunction, deletionIgnorePatterns));
        }

        /// <summary>
        /// Checks whether the configuration for executing an action is valid.
        /// </summary>
        /// <param name="containerName">Container name.</param>
        /// <returns>Check result.</returns>
        private Result CheckConfiguration(string containerName)
        {
            if (string.IsNullOrWhiteSpace(Options.ConnectionString)
                || Options.ExecutionTimeout == 0
                || string.IsNullOrWhiteSpace(containerName))
            {
                string error = string.Format(Properties.Resources.InvalidConfiguration, Options.ConnectionString, Options.ExecutionTimeout, containerName);
                Logger.LogError(error);

                return Result.Fail(error);
            }

            return Result.Ok();
        }

        /// <summary>
        /// Creates an Azure blob or updates its data.
        /// </summary>
        /// <param name="containerName">Name of the container containing the blob to fetch.</param>
        /// <param name="blobName">Name of the blob to fetch.</param>
        /// <param name="data">Data to set.</param>
        /// <param name="httpHeaders">HTTP headers to apply to the updated blob.</param>
        /// <param name="blobContainerClient">Already instanciated blob container client.</param>
        private Result ExecuteCreateOrUpdateBlob(string containerName, string blobName, byte[] data, BlobHttpHeaders? httpHeaders, IBlobContainerClientWrapper? blobContainerClient = null)
        {
            try
            {
                Logger.LogInformation(string.Format(Properties.Resources.UpdatingBlob, blobName, containerName));

                if (blobContainerClient == null)
                {
                    blobContainerClient = BlobContainerClientWrapperFactory.Create(Options.ConnectionString, containerName);
                    blobContainerClient.CreateIfNotExists();
                }

                IBlockBlobClientWrapper blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);

                if (httpHeaders == null)
                {
                    httpHeaders = new BlobHttpHeaders();
                }

                httpHeaders.ContentType = Path.HasExtension(blobName) ? MimeUtility.GetMimeMapping(blobName) : "application/octet-stream";

                using MemoryStream memoryStream = new(data);
                Task updateTask = blockBlobClient.UploadAsync(memoryStream, httpHeaders);

                if (!updateTask.Wait(Options.ExecutionTimeout * 1000))
                {
                    string error = string.Format(Properties.Resources.BlobUpdateExecutionDelayExceeded, Options.ExecutionTimeout, blobName, containerName);
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                Logger.LogInformation(string.Format(Properties.Resources.BlobUpdated, blobName, containerName));

                return Result.Ok();
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.BlobUpdateError, blobName, containerName, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Fetches data from an Azure blob.
        /// </summary>
        /// <param name="containerName">Name of the container containing the blob to fetch.</param>
        /// <param name="blobName">Name of the blob to fetch.</param>
        /// <returns>Blob value.</returns>
        private Result<string> ExecuteFetchBlob(string containerName, string blobName)
        {
            try
            {
                Logger.LogInformation(string.Format(Properties.Resources.FetchingBlob, blobName, containerName));

                IBlobContainerClientWrapper blobContainerClient = BlobContainerClientWrapperFactory.Create(Options.ConnectionString, containerName);
                IBlockBlobClientWrapper blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);

                using MemoryStream memoryStream = new();
                Task fetchTask = blockBlobClient.DownloadToAsync(memoryStream);

                if (!fetchTask.Wait(Options.ExecutionTimeout * 1000))
                {
                    string error = string.Format(Properties.Resources.BlobFetchExecutionDelayExceeded, Options.ExecutionTimeout, blobName, containerName);
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                memoryStream.Flush();
                memoryStream.Position = 0;
                StreamReader streamReader = new(memoryStream);
                string blobData = streamReader.ReadToEnd();

                Logger.LogInformation(string.Format(Properties.Resources.BlobFetched, blobName, containerName));

                return Result.Ok(blobData);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.BlobFetchError, blobName, containerName, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Updates the whole content of a Azure blob container.
        /// New blobs are create, and existing blobs are update or deleted except those included in the ignore pattern.
        /// </summary>
        /// <param name="containerName">Name of the container to update.</param>
        /// <param name="data">List of blob names and their data.</param>
        /// <param name="createHttpHeadersFunction">Function for creating the HTTP headers to apply to the updated blobs.</param>
        /// <param name="deletionIgnorePatterns">Patterns to avoid deleting matching blobs.</param>
        private Result ExecuteUpdateContainer(string containerName, Dictionary<string, byte[]> data, Func<BlobHttpHeaders>? createHttpHeadersFunction, params string[] deletionIgnorePatterns)
        {
            data = data.ToDictionary(kvp => kvp.Key.Replace(Path.DirectorySeparatorChar, '/'), kvp => kvp.Value); // Making sure blob name have the same separators as on Azure

            try
            {
                Logger.LogInformation(string.Format(Properties.Resources.UpdatingContainer, containerName));

                List<string> blobsToDelete = [];
                IBlobContainerClientWrapper blobContainerClient = BlobContainerClientWrapperFactory.Create(Options.ConnectionString, containerName);
                blobContainerClient.CreateIfNotExists();

                foreach (IBlobItemWrapper existingBlob in blobContainerClient.GetBlobs())
                {
                    if (!data.ContainsKey(existingBlob.Name)
                        && (!deletionIgnorePatterns.Any(dip => Regex.IsMatch(existingBlob.Name, dip))))
                    {
                        blobsToDelete.Add(existingBlob.Name);
                    }
                }

                List<Task<Result>> createAndUpdateTasks = [];
                List<Task> deleteTasks = [];

                foreach (string blobName in data.Keys)
                {
                    BlobHttpHeaders? httpHeaders = null;

                    if (createHttpHeadersFunction != null)
                    {
                        httpHeaders = createHttpHeadersFunction();
                    }

                    createAndUpdateTasks.Add(Task.Run(() => ExecuteCreateOrUpdateBlob(containerName, blobName, data[blobName], httpHeaders, blobContainerClient)));
                }

                foreach (string blobName in blobsToDelete)
                {
                    deleteTasks.Add(Task.Run(() =>
                    {
                        Logger.LogInformation(string.Format(Properties.Resources.DeletingBlob, blobName, containerName));

                        IBlockBlobClientWrapper blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);
                        blockBlobClient.DeleteIfExists();
                    }));
                }

                Task.WaitAll(
                    Task.WhenAll(createAndUpdateTasks),
                    Task.WhenAll(deleteTasks));

                if (!createAndUpdateTasks.All(t => t.Result.IsSuccess))
                {
                    return Result.Merge(createAndUpdateTasks.Where(t => !t.Result.IsSuccess).Select(t => t.Result).ToArray());
                }

                Logger.LogInformation(string.Format(Properties.Resources.ContainerUpdated, containerName));

                return Result.Ok();
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ContainerUpdateError, containerName, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }
    }
}

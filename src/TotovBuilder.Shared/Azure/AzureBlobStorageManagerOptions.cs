namespace TotovBuilder.Shared.Azure
{
    /// <summary>
    /// Represents options for a <see cref="AzureBlobStorageManager"/>.
    /// </summary>
    public class AzureBlobStorageManagerOptions
    {
        /// <summary>
        /// Connection string to the Azure blob storage.
        /// </summary>
        public readonly string ConnectionString;

        /// <summary>
        /// Time (in seconds) before an operations made on the Azure blob storage are timed out.
        /// </summary>
        public readonly int ExecutionTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageManagerOptions"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the Azure blob storage</param>
        /// <param name="executionTimeout">Time (in seconds) before an operations made on the Azure blob storage are timed out</param>
        public AzureBlobStorageManagerOptions(string connectionString, int executionTimeout)
        {
            ConnectionString = connectionString;
            ExecutionTimeout = executionTimeout;
        }
    }
}

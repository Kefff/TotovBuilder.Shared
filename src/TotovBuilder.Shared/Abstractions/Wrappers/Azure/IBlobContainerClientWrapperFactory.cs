namespace TotovBuilder.Deployer.Abstractions.Wrappers.Azure
{
    /// <summary>
    /// Provides the functionnalities of a <see cref="IBlobContainerClientWrapper"/> factory.
    /// </summary>
    public interface IBlobContainerClientWrapperFactory
    {

        /// <summary>
        /// Creates an instance of an <see cref="IBlobContainerClientWrapper"/>.
        /// </summary>
        /// <param name="connectionString">
        /// A connection string includes the authentication information
        /// required for your application to access data in an Azure Storage
        /// account at runtime.
        ///
        /// For more information,
        /// <see href="https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string">
        /// Configure Azure Storage connection strings</see>
        /// </param>
        /// <param name="blobContainerName">
        /// The name of the blob container in the storage account to reference.
        /// </param>
        /// <returns>Instance.</returns>
        IBlobContainerClientWrapper Create(string connectionString, string blobContainerName);
    }
}

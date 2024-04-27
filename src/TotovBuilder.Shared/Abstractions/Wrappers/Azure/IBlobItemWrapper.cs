using Azure.Storage.Blobs.Models;

namespace TotovBuilder.Deployer.Abstractions.Wrappers.Azure
{
    /// <summary>
    /// Provides the functionalities of a <see cref="BlobItem"/> wrapper.
    /// </summary>
    public interface IBlobItemWrapper
    {
        /// <summary>
        /// Name.
        /// </summary>
        string Name { get; }
    }
}

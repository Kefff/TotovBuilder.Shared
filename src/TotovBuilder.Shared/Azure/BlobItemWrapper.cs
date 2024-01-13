using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs.Models;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Shared.Azure
{
    /// <summary>
    /// Represents a <see cref="BlobItem"/> wrapper
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the BlobItem class.")]
    public class BlobItemWrapper : IBlobItemWrapper
    {
        /// <inheritdoc/>
        public string Name
        {
            get
            {
                return Instance.Name;
            }
        }

        /// <summary>
        /// Instance.
        /// </summary>
        private BlobItem Instance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobItem"/> class.
        /// </summary>
        /// <param name="instance"></param>
        public BlobItemWrapper(BlobItem instance)
        {
            Instance = instance;
        }
    }
}

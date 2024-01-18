using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Shared.Abstractions.Utils;

namespace TotovBuilder.Shared.Utils
{
    /// <summary>
    /// Represents a <see cref="Directory"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the Directory class.")]
    public class DirectoryWrapper : IDirectoryWrapper
    {
        /// <inheritdoc/>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Shared.Abstractions.Utils;

namespace TotovBuilder.Shared.Utils
{
    /// <summary>
    /// Represents a <see cref="File"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the File class.")]
    public class FileWrapper : IFileWrapper
    {
        /// <inheritdoc/>
        public bool Exists(string? path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        /// <inheritdoc/>
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        /// <inheritdoc/>
        public Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }

        /// <inheritdoc/>
        public void WriteAllText(string path, string? contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}

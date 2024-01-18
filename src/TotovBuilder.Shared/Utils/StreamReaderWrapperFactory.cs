using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Shared.Abstractions.Utils;

namespace TotovBuilder.Shared.Utils
{
    /// <summary>
    /// Represents an <see cref="IStreamReaderWrapperFactory"/> factory.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the StreamReader class.")]
    public class StreamReaderWrapperFactory : IStreamReaderWrapperFactory
    {
        /// <inheritdoc/>
        public IStreamReaderWrapper Create(string path)
        {
            StreamReader streamReader = new StreamReader(path);
            StreamReaderWrapper wrapper = new StreamReaderWrapper(streamReader);

            return wrapper;
        }
    }
}

namespace TotovBuilder.Shared.Abstractions.Utils
{
    /// <summary>
    /// Provides the functionnalities of a <see cref="IStreamReaderWrapperFactory"/> factory.
    /// </summary>
    public interface IStreamReaderWrapperFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="IStreamReaderWrapperFactory"/>.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <returns>Instance.</returns>
        IStreamReaderWrapper Create(string path);
    }
}

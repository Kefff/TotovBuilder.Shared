using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TotovBuilder.Shared.Abstractions.Azure;
using TotovBuilder.Shared.Azure;

namespace TotovBuilder.Shared.Extensions
{
    /// <summary>
    /// Represents extension methods for <see cref="IServiceCollection"/> classes.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the Azure blob manager.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="getOptionsFunction">Function for getting the Azure blob storage options to use.</param>
        /// <returns>Services.</returns>
        public static IServiceCollection AddAzureBlobStorageManager(this IServiceCollection services, Func<IServiceProvider, AzureBlobStorageManagerOptions> getOptionsFunction)
        {
            services.AddSingleton<IAzureBlobStorageManager>((IServiceProvider serviceProvider) =>
            {
                IBlobContainerClientWrapperFactory blobContainerClientWrapperFactory = serviceProvider.GetRequiredService<IBlobContainerClientWrapperFactory>();
                ILogger<AzureBlobStorageManager> logger = serviceProvider.GetRequiredService<ILogger<AzureBlobStorageManager>>();

                return new AzureBlobStorageManager(logger, blobContainerClientWrapperFactory, () => getOptionsFunction(serviceProvider));
            });
            services.AddSingleton<IBlobContainerClientWrapperFactory, BlobContainerClientWrapperFactory>();

            return services;
        }
    }
}

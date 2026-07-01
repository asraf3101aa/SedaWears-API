using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.Extensions;

internal static class S3Extensions
{
    public static IServiceCollection AddS3(this IServiceCollection services)
    {
        services.AddOptions<AmazonS3Config>()
            .Configure<S3Config>((options, config) =>
            {
                options.ForcePathStyle = !string.IsNullOrEmpty(config.Endpoint);
                if (!string.IsNullOrEmpty(config.Endpoint))
                {
                    options.ServiceURL = config.Endpoint;
                }
                else if (!string.IsNullOrEmpty(config.Region))
                {
                    options.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(config.Region);
                }
            });

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<S3Config>>().Value;
            var s3Options = sp.GetRequiredService<IOptions<AmazonS3Config>>().Value;
            return new AmazonS3Client(config.AccessKey, config.SecretKey, s3Options);
        });

        return services;
    }
}

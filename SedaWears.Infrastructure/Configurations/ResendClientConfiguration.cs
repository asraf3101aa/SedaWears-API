using Microsoft.Extensions.Options;
using Resend;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.Configurations;

public class ResendClientConfiguration(ResendConfig config) : IConfigureOptions<ResendClientOptions>
{
    public void Configure(ResendClientOptions options)
    {
        options.ApiToken = config.ApiKey;
    }
}

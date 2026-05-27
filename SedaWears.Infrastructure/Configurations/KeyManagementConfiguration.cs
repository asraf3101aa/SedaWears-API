using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace SedaWears.Infrastructure.Configurations;

public class KeyManagementConfiguration(IConnectionMultiplexer multiplexer) : IConfigureOptions<KeyManagementOptions>
{
    public void Configure(KeyManagementOptions options)
    {
        options.XmlRepository = new RedisXmlRepository(() => multiplexer.GetDatabase(), "DataProtection-Keys");
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace SedaWears.Presentation.Configurations;

public class ForwardedHeadersConfiguration : IConfigureOptions<ForwardedHeadersOptions>
{
    public void Configure(ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor 
                                 | ForwardedHeaders.XForwardedProto 
                                 | ForwardedHeaders.XForwardedHost;
    }
}

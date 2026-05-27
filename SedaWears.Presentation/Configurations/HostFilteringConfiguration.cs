using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Options;

namespace SedaWears.Presentation.Configurations;

public class HostFilteringConfiguration(IConfiguration config, IHostEnvironment env) : IConfigureOptions<HostFilteringOptions>
{
    public void Configure(HostFilteringOptions options)
    {
        var allowedHostsString = config["AllowedHosts"]
            ?? (env.IsDevelopment() ? "*" : string.Empty);

        options.AllowedHosts = allowedHostsString.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

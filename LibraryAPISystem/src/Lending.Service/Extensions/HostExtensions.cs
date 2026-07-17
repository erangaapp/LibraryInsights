using Serilog;

namespace Lending.Service.Extensions;

public static class HostExtensions
{
    public static ConfigureHostBuilder
        ConfigureHost(this ConfigureHostBuilder host)
    {
        host.UseSerilog((ctx, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration));

        return host;
    }
}

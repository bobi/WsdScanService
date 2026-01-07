using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Host.Repositories;
using WsdScanService.Host.Services;

namespace WsdScanService.Host;

public static class WsScanServiceHostExtensions
{
    public static IServiceCollection AddWsScanServiceHostServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<HostIpResolverService>();
        services.AddOptions<WsdScanServiceHostConfiguration>()
            .Bind(config.GetSection(WsdScanServiceHostConfiguration.WsdScanServiceHost))
            .Configure<HostIpResolverService>((configuration, ipResolverService) =>
            {
                if (string.IsNullOrEmpty(configuration.Ip))
                {
                    configuration.Ip = ipResolverService.GetHostIpAddress();
                }
            });

        services.AddSingleton<IDeviceRepository, DeviceRepository>();

        services.AddSingleton<WsEventingClientService>();
        services.AddSingleton<WsTransferClientService>();

        services.AddSingleton<DeviceRegistryService>();
        services.AddHostedService(provider => provider.GetRequiredService<DeviceRegistryService>());

        return services;
    }
}

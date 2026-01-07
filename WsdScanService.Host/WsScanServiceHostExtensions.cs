using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Host.Repositories;
using WsdScanService.Host.Services;

namespace WsdScanService.Host;

public static class WsScanServiceHostExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        services.AddSingleton<HostIpResolverService>();
        services.AddOptions<ScanServiceConfiguration>()
            .Bind(config.GetSection(ScanServiceConfiguration.WsdScanService))
            .Configure<HostIpResolverService>((configuration, ipResolverService) =>
                {
                    if (string.IsNullOrEmpty(configuration.Ip))
                    {
                        configuration.Ip = ipResolverService.GetHostIpAddress();
                    }
                }
            );

        services.AddSingleton<IDeviceRepository, DeviceRepository>();

        services.AddSingleton<IDeviceManager, DeviceManager>()
            .AddHostedService(provider => provider.GetRequiredService<IDeviceManager>());

        services.AddSingleton<IScanJobManager, ScanJobManager>()
            .AddHostedService(provider => provider.GetRequiredService<IScanJobManager>());
        services.AddSingleton<SubscriptionRenewService>()
            .AddHostedService(provider => provider.GetRequiredService<SubscriptionRenewService>());

        return services;
    }
}
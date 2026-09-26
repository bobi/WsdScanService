using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Host.Repositories;
using WsdScanService.Host.Services;
using WsdScanService.Scanner.Services;

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
            )
            .Validate(
                configuration => Directory.Exists(configuration.OutputDir),
                "WsdScanService:OutputDir must point to an existing directory"
            )
            .Validate(
                HasDefaultConverterForSaneFormat,
                "WsdScanService:Sane:ImageConverters:default must be defined when WsdScanService:Sane:Format is set"
            )
            .Validate(
                HasValidConverterExtensions,
                "WsdScanService:*:ImageConverters:*:Extension must contain only letters and digits"
            )
            .ValidateOnStart();

        services.AddSingleton<DeviceRepository>();

        services.AddSingleton<DeviceRemovalTracker>();
        services.AddSingleton<IDeviceManager, DeviceManager>()
            .AddHostedService(provider => provider.GetRequiredService<IDeviceManager>());

        services.AddSingleton<IScanJobManager, ScanJobManager>()
            .AddHostedService(provider => provider.GetRequiredService<IScanJobManager>());
        services.AddSingleton<SubscriptionRenewService>()
            .AddHostedService(provider => provider.GetRequiredService<SubscriptionRenewService>());

        return services;
    }

    // A forced Sane.Format (e.g. pnm) relies on the default converter to produce the final image
    private static bool HasDefaultConverterForSaneFormat(ScanServiceConfiguration configuration)
    {
        var sane = configuration.Sane;

        if (sane is not { UseSaneBackend: true } || string.IsNullOrEmpty(sane.Format))
        {
            return true;
        }

        return sane.ImageConverters?.TryGetValue(ImageConverterService.DefaultImageConverter, out var converter) == true
               && !string.IsNullOrEmpty(converter.Path);
    }

    // Extension becomes part of the saved file name, so it must not carry dots or path separators
    private static bool HasValidConverterExtensions(ScanServiceConfiguration configuration)
    {
        var converters = (configuration.Sane?.ImageConverters?.Values ?? [])
            .Concat(configuration.Wsd?.ImageConverters?.Values ?? []);

        return converters.All(c =>
            c.Extension is null || (c.Extension.Length > 0 && c.Extension.All(char.IsAsciiLetterOrDigit))
        );
    }
}
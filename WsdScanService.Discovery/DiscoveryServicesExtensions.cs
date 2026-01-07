using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WsdScanService.Discovery.Configuration;
using WsdScanService.Discovery.Protocol;
using WsdScanService.Discovery.Services;
using WsdScanService.Discovery.Services.SoapMessageActions;
using UdpClient = WsdScanService.Discovery.Services.UdpClient;

namespace WsdScanService.Discovery;

public static class DiscoveryServicesExtensions
{
    public static IServiceCollection AddWsdDiscoveryServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DiscoveryConfiguration>(config.GetSection(DiscoveryConfiguration.WsDiscovery));

        services.AddKeyedSingleton<ISoapActionHandler, ResolveMatchesActionHandler>(
            ProtocolConstants.Actions.ResolveMatchesAction
        );
        services.AddKeyedSingleton<ISoapActionHandler, HelloMessageHandler>(
            ProtocolConstants.Actions.HelloAction
        );
        services.AddKeyedSingleton<ISoapActionHandler, ByeMessageHandler>(
            ProtocolConstants.Actions.ByeAction
        );
        services.AddKeyedSingleton<ISoapActionHandler, ProbeMatchesActionHandler>(
            ProtocolConstants.Actions.ProbeMatchesAction
        );


        services.AddSingleton<UdpClient>().AddHostedService(provider => provider.GetRequiredService<UdpClient>());

        services.AddSingleton<IUdpMessageProcessor, UdpMessagesProcessor>()
            .AddHostedService(provider => provider.GetRequiredService<IUdpMessageProcessor>());
        
        services.AddHostedService<UdpPublisher>();
        services.AddHostedService<UdpListener>();

        return services;
    }
}
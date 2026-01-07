using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WsdScanService.Common;
using WsdScanService.Discovery.Messages;
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
        services.Configure<Configuration.Configuration>(config.GetSection(Configuration.Configuration.WsDiscovery));

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
        
        
        services.AddSingleton<UdpClient>();
        services.AddHostedService(provider => provider.GetRequiredService<UdpClient>());
        
        services.AddSingleton<DiscoveryPubSub<UdpReceiveResult>>();
        services.AddHostedService(provider => provider.GetRequiredService<DiscoveryPubSub<UdpReceiveResult>>());
        
        services.AddSingleton<DiscoveryPubSub<IMessage>>();
        services.AddHostedService(provider => provider.GetRequiredService<DiscoveryPubSub<IMessage>>());
        
        services.AddSingleton<UdpMessagesProcessor>();
        services.AddHostedService(provider => provider.GetRequiredService<UdpMessagesProcessor>());
        
        services.AddHostedService<UdpPublisher>();
        services.AddHostedService<UdpListener>();
        
        return services;
    }
}
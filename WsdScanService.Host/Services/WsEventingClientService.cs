using System.Xml;
using Windows.Wdp.Scan;
using Microsoft.Extensions.Options;
using WsdScanService.Common;
using WsdScanService.Common.Configuration;
using WsdScanService.Common.Extensions;
using WsdScanService.Contracts;
using WsdScanService.Contracts.Entities;
using WsdScanService.Contracts.Schemas.WsAddressing;
using WsdScanService.Contracts.Schemas.WsEventing;

namespace WsdScanService.Host.Services;

public class WsEventingClientService(
    ILogger<WsEventingClientService> logger,
    IOptions<WsdScanServiceHostConfiguration> hostConfiguration)
{
    public async Task<Subscription> SubscribeAsync(string subscriptionUrl, string subscribeAction)
    {
        await using var client = new WsEventingClient(subscriptionUrl);

        client.AddTraceMessageLogBehavior(logger);

        var soapResponse = await client.SubscribeAsync(CreateSubscribeRequest(subscribeAction));

        var destinationResponse = soapResponse.DestinationResponses.DestinationResponse.First(type =>
            type.ClientContext.Value == hostConfiguration.Value.ScanClientContext);

        return new Subscription
        {
            Action = subscribeAction,
            Identifier = soapResponse.SubscriptionManager.ReferenceParameters!.Identifier,
            Expires = soapResponse.Expires != null
                ? DateTime.Now + XmlConvert.ToTimeSpan(soapResponse.Expires)
                : DateTime.MaxValue,
            DestinationToken = destinationResponse.DestinationToken.Value,
            ClientContext = destinationResponse.ClientContext.Value,
        };
    }

    public async Task UnsubscribeAsync(string subscriptionUrl, string identifier)
    {
        await using var client = new WsEventingClient(subscriptionUrl);

        client.AddTraceMessageLogBehavior(logger);

        var unsubscribeRequest = new UnsubscribeRequest { Identifier = identifier, };
        await client.UnsubscribeAsync(unsubscribeRequest);
    }

    private SubscribeRequest CreateSubscribeRequest(string subscriptionEventType)
    {
        var hostIp = hostConfiguration.Value.Ip;
        var listenPort = hostConfiguration.Value.Port;
        var soapEndpointPath = hostConfiguration.Value.ScanEndpoint;

        var endpoint = $"http://{hostIp}:{listenPort}{soapEndpointPath}";
        var uniqueId = new UniqueId();
        var displayName = hostConfiguration.Value.ScanDisplayName;
        var clientContext = hostConfiguration.Value.ScanClientContext;
        var expires = XmlConvert.ToString(TimeSpan.FromSeconds(hostConfiguration.Value.SubscriptionRenewInterval));

        return new SubscribeRequest
        {
            EndTo = new EndpointReference<EventingReferenceParameters, ReferenceProperties>
            {
                Address = endpoint,
                ReferenceParameters = new EventingReferenceParameters { Identifier = uniqueId.ToString() },
            },
            Delivery = new Delivery
            {
                Mode = Xd.Eventing.DeliveryModesPush,
                NotifyTo = new EndpointReference<EventingReferenceParameters, ReferenceProperties>
                {
                    Address = endpoint,
                    ReferenceParameters = new EventingReferenceParameters { Identifier = uniqueId.ToString() },
                }
            },
            Filter = new Filter
            {
                Dialect = Xd.DeviceProfile.DialectAction,
                Text = [subscriptionEventType]
            },
            ScanDestinations = new ScanDestinationsType
            {
                ScanDestination =
                {
                    new ScanDestinationBaseType
                    {
                        ClientDisplayName = new String127ExtType { Value = displayName },
                        ClientContext = new String255ExtType { Value = clientContext }
                    },
                    new ScanDestinationBaseType
                    {
                        ClientDisplayName = new String127ExtType { Value = displayName + "600" },
                        ClientContext = new String255ExtType { Value = clientContext + "_600" }
                    }
                }
            },
            Expires = expires
        };
    }
}
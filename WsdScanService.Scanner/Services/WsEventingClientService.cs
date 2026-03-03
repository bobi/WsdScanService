using System.Collections.Immutable;
using System.Xml;
using Windows.Wdp.Scan;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Scanner.Contracts;
using WsdScanService.Scanner.WsdSchemas.WsAddressing;
using WsdScanService.Scanner.WsdSchemas.WsEventing;

namespace WsdScanService.Scanner.Services;

internal class WsEventingClientService(
    ILogger<WsEventingClientService> logger,
    IOptions<ScanServiceConfiguration> hostConfiguration)
{
    private readonly Dictionary<SubscriptionEventType, string> _eventTypeToActionMap = new()
    {
        { SubscriptionEventType.ScanAvailableEvent, Xd.ScanService.CallbackActions.ScanAvailableEvent },
        {
            SubscriptionEventType.ScannerElementsChangeEvent,
            Xd.ScanService.CallbackActions.ScannerElementsChangeEvent
        },
        {
            SubscriptionEventType.ScannerStatusSummaryEvent,
            Xd.ScanService.CallbackActions.ScannerStatusSummaryEvent
        },
        {
            SubscriptionEventType.ScannerStatusConditionEvent,
            Xd.ScanService.CallbackActions.ScannerStatusConditionEvent
        },
        {
            SubscriptionEventType.ScannerStatusConditionClear,
            Xd.ScanService.CallbackActions.ScannerStatusConditionClear
        },
        { SubscriptionEventType.JobStatusEvent, Xd.ScanService.CallbackActions.JobStatusEvent },
        { SubscriptionEventType.JobEndStateEvent, Xd.ScanService.CallbackActions.JobEndStateEvent }
    };

    public async Task<Subscription> SubscribeAsync(
        string scanServiceAddress,
        SubscriptionEventType eventType,
        IEnumerable<ScanDestination> scanDestinations
    )
    {
        await using var client = WsEventingClient.Create(scanServiceAddress, logger);

        var soapResponse =
            await client.SubscribeAsync(CreateSubscribeRequest(_eventTypeToActionMap[eventType], scanDestinations));

        return new Subscription
        {
            Identifier = soapResponse.SubscriptionManager.ReferenceParameters!.Identifier,
            Expires = soapResponse.Expires != null
                ? DateTime.Now + XmlConvert.ToTimeSpan(soapResponse.Expires)
                : DateTime.MaxValue,
            DestinationTokens = soapResponse.DestinationResponses?.DestinationResponse?.ToDictionary(
                    e => e.ClientContext.Value,
                    e => e.DestinationToken.Value
                )
                .ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty
        };
    }

    public async Task<DateTime> RenewSubscriptionAsync(string scanServiceAddress, string subscriptionIdentifier)
    {
        await using var client = WsEventingClient.Create(scanServiceAddress, logger);

        var expires = XmlConvert.ToString(TimeSpan.FromSeconds(hostConfiguration.Value.SubscriptionRenewInterval));
        var renewRequest = new RenewRequest
        {
            Identifier = subscriptionIdentifier,
            Expires = expires
        };

        var soapResponse = await client.RenewAsync(renewRequest);

        return soapResponse.Expires != null
            ? DateTime.Now + XmlConvert.ToTimeSpan(soapResponse.Expires)
            : DateTime.MaxValue;
    }

    public async Task UnsubscribeAsync(string scanServiceAddress, string subscriptionIdentifier)
    {
        await using var client = WsEventingClient.Create(scanServiceAddress, logger);

        var unsubscribeRequest = new UnsubscribeRequest { Identifier = subscriptionIdentifier };

        await client.UnsubscribeAsync(unsubscribeRequest);
    }

    private SubscribeRequest CreateSubscribeRequest(
        string subscriptionEventType,
        IEnumerable<ScanDestination> destinations
    )
    {
        var hostIp = hostConfiguration.Value.Ip;
        var listenPort = hostConfiguration.Value.Port;
        var soapEndpointPath = hostConfiguration.Value.ScanEndpoint;

        var endpoint = $"http://{hostIp}:{listenPort}{soapEndpointPath}";
        var uniqueId = new UniqueId();
        var expires = XmlConvert.ToString(TimeSpan.FromSeconds(hostConfiguration.Value.SubscriptionRenewInterval));

        var subscribeRequest = new SubscribeRequest
        {
            EndTo = new EndpointReference<EventingReferenceParameters, ReferenceProperties>
            {
                Address = endpoint,
                ReferenceParameters = new EventingReferenceParameters { Identifier = uniqueId.ToString() }
            },
            Delivery = new Delivery
            {
                Mode = Xd.Eventing.DeliveryModesPush,
                NotifyTo = new EndpointReference<EventingReferenceParameters, ReferenceProperties>
                {
                    Address = endpoint,
                    ReferenceParameters = new EventingReferenceParameters { Identifier = uniqueId.ToString() }
                }
            },
            Filter = new Filter
            {
                Dialect = Xd.DeviceProfile.DialectAction,
                Text = [subscriptionEventType]
            },
            ScanDestinations = new ScanDestinationsType(),
            Expires = expires
        };

        foreach (var destination in destinations)
        {
            subscribeRequest.ScanDestinations.ScanDestination.Add(
                new ScanDestinationBaseType
                {
                    ClientDisplayName = new String127ExtType { Value = destination.DisplayName },
                    ClientContext = new String255ExtType { Value = destination.Id }
                }
            );
        }

        return subscribeRequest;
    }
}
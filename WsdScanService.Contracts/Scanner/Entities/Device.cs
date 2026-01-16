namespace WsdScanService.Contracts.Scanner.Entities;

using System.Collections.Immutable;
using ClientContext = string;

public record Device : ScanDeviceMetadata
{
    public required string DeviceId { get; init; }

    public required string Type { get; init; }

    public required string MexAddress { get; init; }
    
    public uint InstanceId { get; init; }
    
    public uint MetadataVersion { get; init; }

    public IImmutableDictionary<SubscriptionEventType, Subscription> Subscriptions { get; init; } = ImmutableDictionary<SubscriptionEventType, Subscription>.Empty;
    
    public IImmutableDictionary<ClientContext, ScanTicket> ScanTickets { get; init; } = ImmutableDictionary<ClientContext, ScanTicket>.Empty;

    public IImmutableList<ScanDestination> ScanDestinations { get; init; } = ImmutableList<ScanDestination>.Empty;
    
    public override string ToString()
    {
        return
            $"DeviceId: {DeviceId}, MexAddress: {MexAddress}, Type: {Type}, Model: {ModelName}, Serial: {SerialNumber}, InstanceId: {InstanceId}, MetadataVersion: {MetadataVersion}";
    }
}
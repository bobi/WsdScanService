namespace WsdScanService.Contracts.Scanner.Entities;

using ClientContext = string;

public class Device : ScanDeviceMetadata
{
    public required string DeviceId { get; init; }

    public required string Type { get; init; }

    public required string MexAddress { get; init; }
    
    public uint InstanceId { get; set; }
    
    public uint MetadataVersion { get; set; }

    public required IDictionary<SubscriptionEventType, Subscription> Subscriptions { get; init; }
    
    public required IDictionary<ClientContext, ScanTicket> ScanTickets { get; init; }

    public required IEnumerable<ScanDestination> ScanDestinations { get; init; }
    
    public override string ToString()
    {
        return
            $"DeviceId: {DeviceId}, MexAddress: {MexAddress}, Type: {Type}, Model: {ModelName}, Serial: {SerialNumber}, InstanceId: {InstanceId}, MetadataVersion: {MetadataVersion}";
    }
}
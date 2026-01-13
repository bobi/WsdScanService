namespace WsdScanService.Contracts.Scanner.Entities;

using DestinationToken = string;
using ClientContext = string;

public class Subscription
{
    public required string Identifier { get; init; }
    
    public DateTime Expires { get; set; }

    public required IDictionary<ClientContext, DestinationToken> DestinationTokens { get; init; }
}

public enum SubscriptionEventType
{
    ScanAvailableEvent,
    ScannerElementsChangeEvent,
    ScannerStatusSummaryEvent,
    ScannerStatusConditionEvent,
    ScannerStatusConditionClear,
    JobStatusEvent,
    JobEndStateEvent,
}
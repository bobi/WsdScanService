namespace WsdScanService.Contracts.Scanner.Entities;

using System.Collections.Immutable;
using DestinationToken = string;
using ClientContext = string;

public record Subscription
{
    public required string Identifier { get; init; }
    
    public DateTime Expires { get; init; }

    public required IImmutableDictionary<ClientContext, DestinationToken> DestinationTokens { get; init; }
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
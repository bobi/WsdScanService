using System.Collections.Immutable;

namespace WsdScanService.Contracts.Scanner.Entities;

public record Subscription
{
    public required string Identifier { get; init; }

    public DateTime Expires { get; init; }

    /// <summary>
    /// Key: ClientContext, Value: DestinationToken
    /// </summary>
    public required IImmutableDictionary<string, string> DestinationTokens { get; init; }
}

public enum SubscriptionEventType
{
    ScanAvailableEvent,
    ScannerElementsChangeEvent,
    ScannerStatusSummaryEvent,
    ScannerStatusConditionEvent,
    ScannerStatusConditionClear,
    JobStatusEvent,
    JobEndStateEvent
}
namespace WsdScanService.Contracts.Entities;

public class Subscription
{
    public string Action { get; init; }
    public string Identifier { get; init; }
    public string ClientContext { get; init; }
    public DateTime Expires { get; init; }
    public string DestinationToken { get; init; }
}
using System.ServiceModel;
using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Common;
using WsdScanService.Contracts;
using WsdScanService.Scanner.Contracts;
using WsdScanService.Scanner.Wcf;

namespace WsdScanService.Scanner.Extensions;

internal static class ServiceModelClientExtensions
{
    public static void AddTraceMessageLogBehavior<TC>(this ClientBase<TC> client, ILogger logger)
        where TC : class
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            client.Endpoint.EndpointBehaviors.Add(new LoggingEndpointBehaviour(logger));
        }
    }

    public static XmlCaptureBehavior AddXmlCaptureBehavior<TC>(this ClientBase<TC> client) where TC : class
    {
        var xmlCaptureBehavior = new XmlCaptureBehavior();

        client.Endpoint.EndpointBehaviors.Add(xmlCaptureBehavior);

        return xmlCaptureBehavior;
    }

    public static async Task<TR> ExecuteInOperationContextAsync<TC, TR>(this ClientBase<TC> client,
        Func<OperationContext, Task<TR>> cb) where TC : class
    {
        using var _ = new OperationContextScope(client.InnerChannel);

        OperationContext.Current.OutgoingMessageHeaders.MessageId = new UniqueId();
        OperationContext.Current.OutgoingMessageHeaders.ReplyTo = new EndpointAddress(Xd.Addressing200408.Anonymous);

        return await cb(OperationContext.Current);
    }
}
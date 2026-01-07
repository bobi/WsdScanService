using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Common.Wcf;

public class LoggingEndpointBehaviour<T>(ILogger<T> logger) : IEndpointBehavior
{
    private LoggingMessageInspector<T> MessageInspector { get; } = new(logger);

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(MessageInspector);
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }
}

public class LoggingMessageInspector<T>(ILogger<T> logger) : IClientMessageInspector
{
    private ILogger<T> Logger { get; } = logger;

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            using var buffer = reply.CreateBufferedCopy(int.MaxValue);
            Logger.LogTrace(buffer.CreateMessage().ToString());

            reply = buffer.CreateMessage();
        }
    }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            using var buffer = request.CreateBufferedCopy(int.MaxValue);
            Logger.LogTrace(buffer.CreateMessage().ToString());

            request = buffer.CreateMessage();
        }


        return null;
    }
}
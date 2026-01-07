using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Scanner.Wcf;

internal class LoggingEndpointBehaviour(ILogger logger) : IEndpointBehavior
{
    private LoggingMessageInspector MessageInspector { get; } = new(logger);

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

internal class LoggingMessageInspector(ILogger logger) : IClientMessageInspector
{
    private ILogger Logger { get; } = logger;

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
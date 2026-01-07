using System.Collections.ObjectModel;
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Scanner.Wcf;

internal class LoggingServiceBehaviour(ILogger<LoggingServiceBehaviour> logger) : IServiceBehavior
{
    private LoggingErrorHandler<LoggingServiceBehaviour> ErrorHandler { get; } = new(logger);
    private LoggingDispatchMessageInspector<LoggingServiceBehaviour> MessageInspector { get; } = new(logger);

    public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }

    public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
        Collection<ServiceEndpoint> endpoints,
        BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
        foreach (var chDisp in serviceHostBase.ChannelDispatchers)
        {
            if (chDisp is ChannelDispatcher channelDispatcher)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    channelDispatcher.ErrorHandlers.Add(ErrorHandler);
                }

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    foreach (var epDisp in channelDispatcher.Endpoints)
                    {
                        epDisp?.DispatchRuntime?.MessageInspectors.Add(MessageInspector);
                    }
                }
            }
        }
    }
}

internal class LoggingErrorHandler<T>(ILogger<T> logger) : IErrorHandler
{
    private ILogger<T> Logger { get; } = logger;

    public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
    {
        if (Logger.IsEnabled(LogLevel.Error))
        {
            Logger.LogError(
                "An error occurred while processing the request. {Version}, {Fault}, {Error}",
                version,
                fault,
                error
            );
        }
    }

    public bool HandleError(Exception error)
    {
        return true;
    }
}

internal class LoggingDispatchMessageInspector<T>(ILogger<T> logger) : IDispatchMessageInspector
{
    private ILogger<T> Logger { get; } = logger;

    public object? AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            using var buffer = request.CreateBufferedCopy(int.MaxValue);
            Logger.LogTrace(buffer.CreateMessage().ToString());

            request = buffer.CreateMessage();
        }

        return null;
    }

    public void BeforeSendReply(ref Message reply, object correlationState)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            using var buffer = reply.CreateBufferedCopy(int.MaxValue);
            Logger.LogTrace(buffer.CreateMessage().ToString());

            reply = buffer.CreateMessage();
        }
    }
}
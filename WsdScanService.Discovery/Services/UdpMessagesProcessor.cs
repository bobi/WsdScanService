using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Extensions;
using WsdScanService.Discovery.Protocol;
using WsdScanService.Discovery.Services.SoapMessageActions;
using WsdScanService.Discovery.SoapMessages;
using WsdScanService.Discovery.Utils;

namespace WsdScanService.Discovery.Services;

internal class UdpMessagesProcessor(
    ILogger<UdpMessagesProcessor> logger,
    IServiceProvider messageActions) : BackgroundService, IUdpMessageProcessor
{
    private readonly Channel<UdpReceiveResult> _messageQueue = Channel.CreateUnbounded<UdpReceiveResult>();

    private readonly DuplicateDetector<string> _duplicateDetector = new(2048);

    protected override async Task ExecuteAsync(CancellationToken ctsToken)
    {
        logger.LogDebug("Starting UDP message processor loop");

        try
        {
            await foreach (var udpReceiveResult in _messageQueue.Reader.ReadAllAsync(ctsToken))
            {
                ProcessQueuedMessage(udpReceiveResult);
            }
        }
        catch (OperationCanceledException)
        {
        }

        logger.LogDebug("UDP message processor loop completed");
    }

    private void ProcessQueuedMessage(UdpReceiveResult udpReceiveResult)
    {
        try
        {
            var soapMessage = udpReceiveResult.Buffer.DeserializeFromXml<SoapMessage<XmlElement>>();

            var messageId = soapMessage.SoapHeader?.MessageId;

            if (string.IsNullOrEmpty(messageId))
            {
                logger.LogWarning("MessageId is empty. {}", soapMessage);

                return;
            }

            if (_duplicateDetector.AddIfNotDuplicate(messageId))
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace(
                        "Received message from {0}:\n{1}",
                        udpReceiveResult.RemoteEndPoint,
                        Encoding.UTF8.GetString(udpReceiveResult.Buffer)
                    );
                }

                var soapAction = soapMessage.SoapHeader?.Action;
                if (!string.IsNullOrEmpty(soapAction) && ProtocolConstants.Actions.All.Contains(soapAction))
                {
                    var actionHandler = messageActions.GetKeyedService<ISoapActionHandler>(soapAction);

                    if (actionHandler != null)
                    {
                        // Not awaited: a slow handler (device metadata fetch + retries) must not block discovery
                        _ = HandleAsync(actionHandler, soapAction, udpReceiveResult.Buffer);
                    }
                }
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Duplicate message detected. MessageId: {MessageId}", messageId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private async Task HandleAsync(ISoapActionHandler actionHandler, string soapAction, byte[] buffer)
    {
        try
        {
            await actionHandler.HandleAsync(buffer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle {SoapAction} message", soapAction);
        }
    }

    public void ProcessMessage(UdpReceiveResult udpReceiveResult)
    {
        _messageQueue.Writer.TryWrite(udpReceiveResult);
    }
}
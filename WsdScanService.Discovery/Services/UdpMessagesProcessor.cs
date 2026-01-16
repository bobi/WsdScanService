using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
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
    private readonly BlockingCollection<UdpReceiveResult> _messageQueue = new();

    private readonly DuplicateDetector<string> _duplicateDetector = new(2048);

    protected override Task ExecuteAsync(CancellationToken ctsToken)
    {
        logger.LogDebug("Starting UDP message processor loop");

        while (!ctsToken.IsCancellationRequested)
        {
            try
            {
                var udpReceiveResult = _messageQueue.Take(ctsToken);

                var soapMessage = udpReceiveResult.Buffer.DeserializeFromXml<SoapMessage<XmlElement>>();

                var messageId = soapMessage.SoapHeader?.MessageId;

                if (string.IsNullOrEmpty(messageId))
                {
                    logger.LogWarning("MessageId is empty. {}", soapMessage);

                    continue;
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

                        actionHandler?.HandleAsync(udpReceiveResult.Buffer);
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        logger.LogDebug("UDP message processor loop completed");

        return Task.CompletedTask;
    }

    public void ProcessMessage(UdpReceiveResult udpReceiveResult)
    {
        _messageQueue.Add(udpReceiveResult);
    }
}
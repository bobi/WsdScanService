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

public class UdpMessagesProcessor(
    ILogger<UdpMessagesProcessor> logger,
    DiscoveryPubSub<UdpReceiveResult> pubSub,
    IServiceProvider messageActions) : IHostedService
{
    private Guid _subscriberId = Guid.Empty;

    private readonly DuplicateDetector<string> _duplicateDetector = new(2048);

    private async Task ProcessMessage(UdpReceiveResult udpReceiveResult, CancellationToken ctsToken)
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
                    await actionHandler.HandleAsync(udpReceiveResult.Buffer, ctsToken);
                }
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriberId = pubSub.Subscribe(ProcessMessage);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        pubSub.Unsubscribe(_subscriberId);

        return Task.CompletedTask;
    }
}
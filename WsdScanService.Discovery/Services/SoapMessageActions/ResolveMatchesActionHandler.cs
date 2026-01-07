using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Common;
using WsdScanService.Common.Extensions;
using WsdScanService.Discovery.Messages;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

public class ResolveMatchesActionHandler(ILogger<ResolveMatchesActionHandler> logger, DiscoveryPubSub<IMessage> pubSub)
    : ISoapActionHandler
{
    public async Task HandleAsync(ReadOnlyMemory<byte> data, CancellationToken ctsToken)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<ResolveMatchesBody>>();

        var soapBody = soapMessage.SoapBody;
        if (soapBody != null)
        {
            var resolveMatch = soapBody.ResolveMatches?.ResolveMatch;

            if (resolveMatch != null)
            {
                var types = XmlUtils.ParseTypes(resolveMatch.Types, xmlDoc.DocumentElement!.GetNamespaceOfPrefix);

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Got types: [{XmlQualifiedNames}]",
                        string.Join(", ", types?.Select(t => t.ToString()) ?? [])
                    );
                }

                foreach (var type in types ?? [])
                {
                    if (type == HelloMessageHandler.ScanDeviceTypeQName)
                    {
                        var addrs = resolveMatch.XAddrs?.Split(' ');
                        var deviceId = resolveMatch.EndpointReference?.Address;

                        if (addrs is { Length: > 0 } && !string.IsNullOrEmpty(deviceId))
                        {
                            await pubSub.PublishAsync(new AddDevice(deviceId, addrs[0], type.Name), ctsToken);
                        }
                    }
                }
            }
        }
    }
}
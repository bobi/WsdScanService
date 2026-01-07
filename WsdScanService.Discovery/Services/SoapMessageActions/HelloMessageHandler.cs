using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Common;
using WsdScanService.Common.Extensions;
using WsdScanService.Discovery.Messages;
using WsdScanService.Discovery.Protocol;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

public class HelloMessageHandler(ILogger<UdpListener> logger, DiscoveryPubSub<IMessage> pubSub) : ISoapActionHandler
{
    internal static readonly XmlQualifiedName ScanDeviceTypeQName =
        new("ScanDeviceType", ProtocolConstants.Namespaces.WsScanner);

    public async Task HandleAsync(ReadOnlyMemory<byte> data, CancellationToken ctsToken)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<HelloBody>>();
        var hello = soapMessage.SoapBody?.Hello;

        if (hello != null)
        {
            var types = XmlUtils.ParseTypes(hello.Types, xmlDoc.GetNamespaceOfPrefix);

            logger.LogDebug("Got types: {XmlQualifiedNames}", (object?)types);

            foreach (var type in types ?? [])
            {
                if (type == ScanDeviceTypeQName)
                {
                    var addrs = hello.XAddrs?.Split(' ');
                    var deviceId = hello.EndpointReference?.Address;

                    if (addrs is { Length: > 0 } && !string.IsNullOrEmpty(deviceId))
                    {
                        await pubSub.PublishAsync(new AddDevice(deviceId, addrs[0], type.Name), ctsToken);
                    }
                }
            }
        }
    }
}
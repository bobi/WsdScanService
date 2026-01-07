using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Extensions;
using WsdScanService.Common.Utils;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Discovery.Protocol;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

internal class HelloMessageHandler(ILogger<UdpListener> logger, IDeviceManager deviceManager) : ISoapActionHandler
{
    internal static readonly XmlQualifiedName ScanDeviceTypeQName =
        new("ScanDeviceType", ProtocolConstants.Namespaces.WsScanner);

    public async Task HandleAsync(ReadOnlyMemory<byte> data)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<HelloBody>>();
        var hello = soapMessage.SoapBody?.Hello;
        var appSequence = soapMessage.SoapHeader?.AppSequence;

        if (hello != null && appSequence != null)
        {
            var types = XmlUtils.ParseTypes(hello.Types,  xmlDoc.GetAllNamespaces());

            logger.LogDebug("Got types: {XmlQualifiedNames}", (object?)types);

            foreach (var type in types ?? [])
            {
                if (type == ScanDeviceTypeQName)
                {
                    var addrs = hello.XAddrs?.Split(' ');
                    var deviceId = hello.EndpointReference?.Address;

                    if (addrs is { Length: > 0 } && !string.IsNullOrEmpty(deviceId))
                    {
                        await deviceManager.AddDevice(deviceId, addrs[0], type.Name, appSequence.InstanceId, hello.MetadataVersion ?? 0);
                    }
                }
            }
        }
    }
}
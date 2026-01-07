using System.Xml;
using WsdScanService.Common.Extensions;
using WsdScanService.Discovery.Messages;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

public class ByeMessageHandler(DiscoveryPubSub<IMessage> pubSub) : ISoapActionHandler
{
    public async Task HandleAsync(ReadOnlyMemory<byte> data, CancellationToken ctsToken)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<ByeBody>>();

        var bye = soapMessage.SoapBody?.Bye;

        if (bye != null)
        {
            var deviceId = bye.EndpointReference?.Address;

            if (!string.IsNullOrEmpty(deviceId))
            {
                await pubSub.PublishAsync(new RemoveDevice(deviceId), ctsToken);
            }
        }
    }
}
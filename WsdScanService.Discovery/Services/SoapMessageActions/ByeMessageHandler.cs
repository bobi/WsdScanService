using System.Xml;
using WsdScanService.Common.Extensions;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

internal class ByeMessageHandler(IDeviceManager deviceManager) : ISoapActionHandler
{
    public async Task HandleAsync(ReadOnlyMemory<byte> data)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<ByeBody>>();

        var bye = soapMessage.SoapBody?.Bye;
        var appSequence = soapMessage.SoapHeader?.AppSequence;

        if (bye != null && appSequence != null)
        {
            var deviceId = bye.EndpointReference?.Address;

            if (!string.IsNullOrEmpty(deviceId))
            {
                await deviceManager.RemoveDevice(deviceId, appSequence.InstanceId);
            }
        }
    }
}
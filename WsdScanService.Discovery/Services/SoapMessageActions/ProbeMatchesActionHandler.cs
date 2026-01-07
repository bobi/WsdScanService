using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Extensions;
using WsdScanService.Common.Utils;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

internal class ProbeMatchesActionHandler(ILogger<ProbeMatchesActionHandler> logger, IDeviceManager deviceManager)
    : ISoapActionHandler
{
    public async Task HandleAsync(ReadOnlyMemory<byte> data)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<ProbeMatchesBody>>();
        var appSequence = soapMessage.SoapHeader?.AppSequence;

        var soapBody = soapMessage.SoapBody;
        if (soapBody != null && appSequence != null)
        {
            var probeMatches = soapBody.ProbeMatches;

            foreach (var probeMatch in probeMatches?.Matches ?? [])
            {
                var types = XmlUtils.ParseTypes(probeMatch.Types, xmlDoc.GetAllNamespaces());

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
                        var addrs = probeMatch.XAddrs?.Split(' ');
                        var deviceId = probeMatch.EndpointReference?.Address;

                        if (addrs is { Length: > 0 } && !string.IsNullOrEmpty(deviceId))
                        {
                            await deviceManager.AddDevice(deviceId, addrs[0], type.Name, appSequence.InstanceId, probeMatch.MetadataVersion ?? 0);
                        }
                    }
                }
            }
        }
    }
}
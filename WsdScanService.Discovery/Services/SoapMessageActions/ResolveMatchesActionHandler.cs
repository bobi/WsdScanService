using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Extensions;
using WsdScanService.Common.Utils;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services.SoapMessageActions;

internal class ResolveMatchesActionHandler(ILogger<ResolveMatchesActionHandler> logger, IDeviceManager deviceManager)
    : ISoapActionHandler
{
    public async Task HandleAsync(ReadOnlyMemory<byte> data)
    {
        var xmlDoc = data.DeserializeFromXml<XmlDocument>();
        var soapMessage = xmlDoc.Deserialize<SoapMessage<ResolveMatchesBody>>();
        var appSequence = soapMessage.SoapHeader?.AppSequence;

        var soapBody = soapMessage.SoapBody;
        var resolveMatch = soapBody?.ResolveMatches?.ResolveMatch;

        if (resolveMatch != null && appSequence != null)
        {
            var types = XmlUtils.ParseTypes(resolveMatch.Types, xmlDoc.GetAllNamespaces());

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
                        await deviceManager.AddDevice(deviceId, addrs[0], type.Name, appSequence.InstanceId, resolveMatch.MetadataVersion ?? 0);
                    }
                }
            }
        }
    }
}
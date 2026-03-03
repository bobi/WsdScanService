using System.Xml;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Extensions;
using WsdScanService.Discovery.Protocol;
using WsdScanService.Discovery.SoapMessages;

namespace WsdScanService.Discovery.Services;

internal class UdpPublisher(
    ILogger<UdpPublisher> logger,
    IOptions<Configuration.DiscoveryConfiguration> configuration,
    UdpClient udpClient) : BackgroundService
{
    private static readonly string ProbeMessageId = $"urn:uuid:{Guid.NewGuid()}";

    private readonly TimeSpan _probeInitialDelay = configuration.Value.ProbeInitialDelay;
    private readonly TimeSpan _probeRepeatDelay = configuration.Value.ProbeRepeatDelay;
    private readonly int _probeProbeRepeatTimes = configuration.Value.ProbeRepeatTimes;
    private readonly uint _instanceId = configuration.Value.InstanceId;

    protected override async Task ExecuteAsync(CancellationToken ctsToken)
    {
        if (_probeInitialDelay.TotalSeconds > 0)
        {
            await Task.Delay(_probeInitialDelay, ctsToken);
        }

        var infiniteProbe = _probeProbeRepeatTimes < 0;
        var repeatTimes = _probeProbeRepeatTimes;
        while (!ctsToken.IsCancellationRequested && (infiniteProbe || repeatTimes > 0))
        {
            try
            {
                var probeMessage = SoapMessage<ProbeBody>.Create(
                    SoapHeader.Create(ProtocolConstants.Actions.ProbeAction, ProbeMessageId, _instanceId),
                    ProbeBody.Create(
                        [
                            new XmlQualifiedName("Device", ProtocolConstants.Namespaces.DevicesProfile)
                        ]
                    )
                );

                var data = probeMessage.SerializeToXml(DiscoveryXmlSerializerNamespaces.Namespaces);

                await udpClient.SendAsync(data, ctsToken);

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Sent Probe message.");
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

            if (!infiniteProbe)
                repeatTimes--;

            await Task.Delay(_probeRepeatDelay, ctsToken);
        }
    }
}
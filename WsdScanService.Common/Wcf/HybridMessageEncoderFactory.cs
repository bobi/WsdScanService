using System.ServiceModel.Channels;

namespace WsdScanService.Common.Wcf;

public class HybridMessageEncoderFactory(MessageEncoder encoder, MessageVersion messageVersion) : MessageEncoderFactory
{
    public override MessageEncoder Encoder { get; } = encoder;

    public override MessageVersion MessageVersion { get; } = messageVersion;
}
namespace WsdScanService.Common.Wcf;

using System.IO;
using System.ServiceModel.Channels;

public class HybridMessageEncoder : MessageEncoder
{
    private readonly MessageEncoder _textEncoder;
    private readonly IList<MessageEncoder> _otherEncoders;

    public HybridMessageEncoder(MessageEncoderFactory textFactory,
        MessageEncoderFactory[] otherFactories,
        MessageVersion messageVersion)
    {
        _textEncoder = textFactory.Encoder;
        _otherEncoders = new List<MessageEncoder>(otherFactories.Select(e => e.Encoder));
        MessageVersion = messageVersion;
    }

    public override string ContentType => _textEncoder.ContentType;

    public override bool IsContentTypeSupported(string contentType)
    {
        return _otherEncoders.Any(e => e.IsContentTypeSupported(contentType)) ||
               _textEncoder.IsContentTypeSupported(contentType);
    }

    public override void WriteMessage(Message message, Stream stream)
    {
        _textEncoder.WriteMessage(message, stream);
    }

    public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager,
        int messageOffset)
    {
        return _textEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
    }

    public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
    {
        var messageEncoder = _otherEncoders.FirstOrDefault(e => e.IsContentTypeSupported(contentType));

        if (messageEncoder != null)
        {
            return messageEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
        }

        return _textEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
    }

    public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
    {
        var messageEncoder = _otherEncoders.FirstOrDefault(e => e.IsContentTypeSupported(contentType));

        if (messageEncoder != null)
        {
            return messageEncoder.ReadMessage(buffer, bufferManager, contentType);
        }

        return _textEncoder.ReadMessage(buffer, bufferManager, contentType);
    }

    public override string MediaType => _textEncoder.MediaType;
    public override MessageVersion MessageVersion { get; }
}
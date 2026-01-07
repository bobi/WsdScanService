using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace WsdScanService.Common.Wcf;

public class HybridEncodingBindingElement : MessageEncodingBindingElement
{
    private readonly TextMessageEncodingBindingElement _text;
    private readonly MtomMessageEncodingBindingElement _mtom;
    private readonly BinaryMessageEncodingBindingElement _binary;

    private MessageVersion _messageVersion;

    public HybridEncodingBindingElement()
    {
        _messageVersion = MessageVersion.Soap12WSAddressingAugust2004;
        var writeEncoding = Encoding.UTF8;

        _text = new TextMessageEncodingBindingElement(_messageVersion, writeEncoding);
        XmlDictionaryReaderQuotas.Max.CopyTo(_text.ReaderQuotas);

        _mtom = new MtomMessageEncodingBindingElement(_messageVersion, writeEncoding);
        XmlDictionaryReaderQuotas.Max.CopyTo(_mtom.ReaderQuotas);

        _binary = new BinaryMessageEncodingBindingElement();
        XmlDictionaryReaderQuotas.Max.CopyTo(_binary.ReaderQuotas);
    }

    private HybridEncodingBindingElement(HybridEncodingBindingElement elementToBeCloned)
    {
        _text = (TextMessageEncodingBindingElement)elementToBeCloned._text.Clone();
        _mtom = (MtomMessageEncodingBindingElement)elementToBeCloned._mtom.Clone();
        _binary = (BinaryMessageEncodingBindingElement)elementToBeCloned._binary.Clone();
        _messageVersion = elementToBeCloned._messageVersion;
    }

    public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
    {
        context.BindingParameters.Add(this);
        return context.BuildInnerChannelFactory<TChannel>();
    }

    public override MessageEncoderFactory CreateMessageEncoderFactory()
    {
        return new HybridMessageEncoderFactory(
            new HybridMessageEncoder(
                _text.CreateMessageEncoderFactory(),
                [
                    _mtom.CreateMessageEncoderFactory(),
                    _binary.CreateMessageEncoderFactory()
                ],
                _messageVersion
            ),
            _messageVersion
        );
    }

    public override MessageVersion MessageVersion
    {
        get => _messageVersion;
        set => _messageVersion = value;
    }

    public override BindingElement Clone()
    {
        return new HybridEncodingBindingElement(this);
    }

    public override T GetProperty<T>(BindingContext context)
    {
        return _text.GetProperty<T>(context) ??
               _mtom.GetProperty<T>(context) ?? _binary.GetProperty<T>(context) ?? base.GetProperty<T>(context);
    }
}
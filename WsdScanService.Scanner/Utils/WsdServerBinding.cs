using CoreWCF.Channels;

namespace WsdScanService.Scanner.Utils;

internal class WsdServerBinding : CustomBinding
{
    public WsdServerBinding(bool manualAddressing = false)
    {
        var textMessageEncoding = new TextMessageEncodingBindingElement
        {
            MessageVersion = MessageVersion.Soap12WSAddressingAugust2004
        };

        var httpTransport = new HttpTransportBindingElement
        {
            ManualAddressing = manualAddressing,
        };

        Elements.Add(textMessageEncoding);
        Elements.Add(httpTransport);
    }
}
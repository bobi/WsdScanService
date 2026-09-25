using System.ServiceModel.Channels;
using WsdScanService.Scanner.Wcf;

namespace WsdScanService.Scanner.Utils;

internal class WsdClientBinding : CustomBinding
{
    private const int MaxBufferSize = int.MaxValue;
    private const int MaxReceivedMessageSize = int.MaxValue;

    private const bool BypassProxyOnLocal = false;
    private const bool UseDefaultWebProxy = false;
    private readonly Uri? _proxyAddress = null;

    public WsdClientBinding(bool manualAddressing = false, TimeSpan? operationTimeout = null)
    {
        if (operationTimeout is { } timeout)
        {
            SendTimeout = timeout;
            ReceiveTimeout = timeout;
        }

        var httpTransport = new HttpTransportBindingElement
        {
            MaxBufferSize = MaxBufferSize,
            MaxReceivedMessageSize = MaxReceivedMessageSize,
            AllowCookies = true,
            ManualAddressing = manualAddressing
        };

        if (_proxyAddress != null)
        {
            httpTransport.ProxyAddress = _proxyAddress;
            httpTransport.BypassProxyOnLocal = BypassProxyOnLocal;
            httpTransport.UseDefaultWebProxy = UseDefaultWebProxy;
        }

        Elements.Add(new HybridEncodingBindingElement());
        Elements.Add(httpTransport);
    }
}
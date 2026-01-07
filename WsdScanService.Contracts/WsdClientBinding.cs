using System.ServiceModel.Channels;
using WsdScanService.Common.Wcf;

namespace WsdScanService.Contracts;

public class WsdClientBinding : CustomBinding
{
    private const int MaxBufferSize = int.MaxValue;
    private const int MaxReceivedMessageSize = int.MaxValue;

    private const bool BypassProxyOnLocal = false;
    private const bool UseDefaultWebProxy = false;
    private readonly Uri? _proxyAddress = null;
    // private readonly Uri _proxyAddress = new("http://127.0.0.1:8080");

    public WsdClientBinding(bool manualAddressing = false)
    {
        ReceiveTimeout = new TimeSpan(0, 10, 0);
        SendTimeout = new TimeSpan(0, 10, 0); 

        var httpTransport = new HttpTransportBindingElement
        {
            MaxBufferSize = MaxBufferSize,
            MaxReceivedMessageSize = MaxReceivedMessageSize,
            AllowCookies = true,
            ManualAddressing = manualAddressing,
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
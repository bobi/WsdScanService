using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using WsdScanService.Common.Extensions;
using WsdScanService.Scanner.Extensions;

namespace WsdScanService.Scanner.Wcf;

internal class XmlCaptureBehavior : IEndpointBehavior
{
    public XmlCaptureInspector Inspector { get; } = new();

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(Inspector);
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }
}

internal class XmlCaptureInspector : IClientMessageInspector
{
    public XmlDocument? LastRequestXml { get; private set; }
    public XmlDocument? LastResponseXml { get; private set; }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        var buffer = request.CreateBufferedCopy(int.MaxValue);

        LastRequestXml = buffer.CreateMessage().ToXmlDocument();

        request = buffer.CreateMessage();

        return null;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var buffer = reply.CreateBufferedCopy(int.MaxValue);

        LastResponseXml = buffer.CreateMessage().ToXmlDocument();

        reply = buffer.CreateMessage();
    }
}
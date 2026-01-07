using System.ServiceModel;
using System.Xml;
using Microsoft.Extensions.Logging;
using WsdScanService.Scanner.Extensions;
using WsdScanService.Scanner.Utils;
using WsdScanService.Scanner.WsdSchemas.WsEventing;

namespace WsdScanService.Scanner.Contracts
{
    internal static partial class Constants
    {
        internal const string EventingContractName = "WsdScanService.Contracts.IWsEventing";
    }

    [XmlSerializerFormat]
    [ServiceContract(ConfigurationName = Constants.EventingContractName, Name = Constants.EventingContractName,
        Namespace = Xd.Eventing.Namespace)]
    internal interface IWsEventing
    {
        [OperationContract(Action = Xd.Eventing.Actions.Subscribe, ReplyAction = Xd.Eventing.Reply.Subscribe)]
        SubscribeResponse Subscribe(SubscribeRequest request);

        [OperationContract(Action = Xd.Eventing.Actions.Subscribe, ReplyAction = Xd.Eventing.Reply.Subscribe)]
        Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request);

        [OperationContract(Action = Xd.Eventing.Actions.Unsubscribe, ReplyAction = Xd.Eventing.Reply.Unsubscribe)]
        UnsubscribeResponse Unsubscribe(UnsubscribeRequest request);

        [OperationContract(Action = Xd.Eventing.Actions.Unsubscribe, ReplyAction = Xd.Eventing.Reply.Unsubscribe)]
        Task<UnsubscribeResponse> UnsubscribeAsync(UnsubscribeRequest request);

        [OperationContract(Action = Xd.Eventing.Actions.Renew, ReplyAction = Xd.Eventing.Reply.Renew)]
        RenewResponse Renew(RenewRequest request);

        [OperationContract(Action = Xd.Eventing.Actions.Renew, ReplyAction = Xd.Eventing.Reply.Renew)]
        Task<RenewResponse> RenewAsync(RenewRequest request);

        [OperationContract(Action = Xd.Eventing.Actions.GetStatus, ReplyAction = Xd.Eventing.Reply.GetStatus)]
        XmlDocument GetStatus();

        [OperationContract(Action = Xd.Eventing.Actions.GetStatus, ReplyAction = Xd.Eventing.Reply.GetStatus)]
        Task<XmlDocument> GetStatusAsync();
    }

    internal class WsEventingClient : ClientBase<IWsEventing>, IWsEventing
    {
        private WsEventingClient(string uri) : base(new WsdClientBinding(), new EndpointAddress(uri))
        {
        }

        public static WsEventingClient Create(string uri, ILogger logger)
        {
            var client = new WsEventingClient(uri);

            client.AddTraceMessageLogBehavior(logger);

            return client;
        }

        public SubscribeResponse Subscribe(SubscribeRequest request)
        {
            return Channel.Subscribe(request);
        }

        public Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request)
        {
            return Channel.SubscribeAsync(request);
        }

        public UnsubscribeResponse Unsubscribe(UnsubscribeRequest request)
        {
            return Channel.Unsubscribe(request);
        }

        public Task<UnsubscribeResponse> UnsubscribeAsync(UnsubscribeRequest request)
        {
            return Channel.UnsubscribeAsync(request);
        }

        public RenewResponse Renew(RenewRequest request)
        {
            return Channel.Renew(request);
        }

        public Task<RenewResponse> RenewAsync(RenewRequest request)
        {
            return Channel.RenewAsync(request);
        }

        public XmlDocument GetStatus()
        {
            return Channel.GetStatus();
        }

        public Task<XmlDocument> GetStatusAsync()
        {
            return Channel.GetStatusAsync();
        }
    }
}
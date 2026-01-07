using System.ServiceModel;
using System.Xml;
using WsdScanService.Common;
using WsdScanService.Contracts.Schemas.WsEventing;

namespace WsdScanService.Contracts
{
    internal static partial class Constants
    {
        internal const string EventingContractName = "WsdScanService.Contracts.IWsEventing";
    }

    [XmlSerializerFormat]
    [ServiceContract(ConfigurationName = Constants.EventingContractName, Name = Constants.EventingContractName,
        Namespace = Xd.Eventing.Namespace)]
    public interface IWsEventing
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
        XmlDocument Renew();

        [OperationContract(Action = Xd.Eventing.Actions.Renew, ReplyAction = Xd.Eventing.Reply.Renew)]
        Task<XmlDocument> RenewAsync();

        [OperationContract(Action = Xd.Eventing.Actions.GetStatus, ReplyAction = Xd.Eventing.Reply.GetStatus)]
        XmlDocument GetStatus();

        [OperationContract(Action = Xd.Eventing.Actions.GetStatus, ReplyAction = Xd.Eventing.Reply.GetStatus)]
        Task<XmlDocument> GetStatusAsync();
    }

    public class WsEventingClient(string uri)
        : ClientBase<IWsEventing>(new WsdClientBinding(), new EndpointAddress(uri)), IWsEventing
    {
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

        public XmlDocument Renew()
        {
            return Channel.Renew();
        }

        public Task<XmlDocument> RenewAsync()
        {
            return Channel.RenewAsync();
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
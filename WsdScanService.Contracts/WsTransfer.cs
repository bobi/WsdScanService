using System.ServiceModel;
using WsdScanService.Common;
using WsdScanService.Contracts.Schemas.WsTransfer;

namespace WsdScanService.Contracts
{
    internal static partial class Constants
    {
        internal const string MexContractName = "WsdScanService.Contracts.IWsTransfer";
    }

    [XmlSerializerFormat]
    [ServiceContract(ConfigurationName = Constants.MexContractName, Name = Constants.MexContractName,
        Namespace = Xd.Transfer.Namespace)]
    public interface IWsTransfer
    {
        [OperationContract(Action = Xd.Transfer.Actions.Get, ReplyAction = Xd.Transfer.Reply.Get)]
        GetResponse Get();

        [OperationContract(Action = Xd.Transfer.Actions.Get, ReplyAction = Xd.Transfer.Reply.Get)]
        Task<GetResponse> GetAsync();
    }

    public class WsTransferClient(string uri)
        : ClientBase<IWsTransfer>(new WsdClientBinding(true), new EndpointAddress(uri)), IWsTransfer
    {
        public GetResponse Get()
        {
            return Channel.Get();
        }

        public Task<GetResponse> GetAsync()
        {
            return Channel.GetAsync();
        }
    }
}
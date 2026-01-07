using System.ServiceModel;
using Microsoft.Extensions.Logging;
using WsdScanService.Scanner.Extensions;
using WsdScanService.Scanner.Utils;
using WsdScanService.Scanner.WsdSchemas.WsTransfer;

namespace WsdScanService.Scanner.Contracts
{
    internal static partial class Constants
    {
        internal const string MexContractName = "WsdScanService.Contracts.IWsTransfer";
    }

    [XmlSerializerFormat]
    [ServiceContract(ConfigurationName = Constants.MexContractName, Name = Constants.MexContractName,
        Namespace = Xd.Transfer.Namespace)]
    internal interface IWsTransferClient
    {
        [OperationContract(Action = Xd.Transfer.Actions.Get, ReplyAction = Xd.Transfer.Reply.Get)]
        GetResponse Get();

        [OperationContract(Action = Xd.Transfer.Actions.Get, ReplyAction = Xd.Transfer.Reply.Get)]
        Task<GetResponse> GetAsync();
    }

    internal class WsTransferClient : ClientBase<IWsTransferClient>, IWsTransferClient
    {
        private WsTransferClient(string uri) : base(new WsdClientBinding(true), new EndpointAddress(uri))
        {
        }

        public static WsTransferClient Create(string uri, ILogger logger)
        {
            var client = new WsTransferClient(uri);

            client.AddTraceMessageLogBehavior(logger);

            return client;
        }

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
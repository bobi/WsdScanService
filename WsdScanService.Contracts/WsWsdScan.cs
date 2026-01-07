using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using WsdScanService.Common;
using WsdScanService.Contracts.ScanService;

namespace WsdScanService.Contracts;

internal static partial class Constants
{
    internal const string ScannerServiceContractName = "WsdScanService.Contracts.IWsTransfer";

    internal const string ScannerServiceCallbackContractName = "WsdScanService.Contracts.IWsWsdScannerServiceCallback";
}

[ServiceContract(
    ConfigurationName = Constants.ScannerServiceContractName,
    Name = Constants.ScannerServiceContractName,
    Namespace = Xd.ScanService.Namespace)]
[XmlSerializerFormat(SupportFaults = true)]
public interface IWsWsdScannerService
{
    [OperationContract(Action = Xd.ScanService.Actions.CreateScanJob, ReplyAction = "*")]
    Task<CreateScanJobResponse> CreateScanJobAsync(CreateScanJobRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.RetrieveImage, ReplyAction = "*")]
    Task<RetrieveImageResponse> RetrieveImageAsync(RetrieveImageRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.CancelJob, ReplyAction = "*")]
    Task<CancelJobResponse> CancelJobAsync(CancelJobRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.ValidateScanTicket, ReplyAction = "*")]
    Task<ValidateScanTicketResponse> ValidateScanTicketAsync(ValidateScanTicketRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.GetScannerElements, ReplyAction = "*")]
    Task<GetScannerElementsResponse> GetScannerElementsAsync(GetScannerElementsRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.GetJobElements, ReplyAction = "*")]
    Task<GetJobElementsResponse> GetJobElementsAsync(GetJobElementsRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.GetActiveJobs, ReplyAction = "*")]
    Task<GetActiveJobsResponse> GetActiveJobsAsync(GetActiveJobsRequest request);

    [OperationContract(Action = Xd.ScanService.Actions.GetJobHistory, ReplyAction = "*")]
    Task<GetJobHistoryResponse> GetJobHistoryAsync(GetJobHistoryRequest request);
}

[ServiceContract(
    ConfigurationName = Constants.ScannerServiceCallbackContractName,
    Name = Constants.ScannerServiceCallbackContractName,
    Namespace = Xd.ScanService.Namespace)]
[XmlSerializerFormat(SupportFaults = true)]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IWsWsdScannerServiceCallback
{
    [OperationContract(Action = Xd.ScanService.CallbackActions.ScanAvailableEvent,
        IsOneWay = false)]
    Task ScanAvailableEventAsync(ScanAvailableEvent request);

    [OperationContract(Action = Xd.ScanService.CallbackActions.ScannerElementsChangeEvent,
        IsOneWay = true)]
    Task ScannerElementsChangeEventAsync(ScannerElementsChangeEvent request);

    [OperationContract(Action = Xd.ScanService.CallbackActions.ScannerStatusSummaryEvent,
        IsOneWay = true)]
    Task ScannerStatusSummaryEventAsync(ScannerStatusSummaryEvent request);

    [CoreWCF.OperationContract(Action = Xd.ScanService.CallbackActions.ScannerStatusConditionEvent,
        IsOneWay = true)]
    Task ScannerStatusConditionEventAsync(ScannerStatusConditionEvent request);

    [OperationContract(Action = Xd.ScanService.CallbackActions.ScannerStatusConditionClear,
        IsOneWay = true)]
    Task ScannerStatusConditionClearedEventAsync(ScannerStatusConditionClearedEvent request);

    [OperationContract(Action = Xd.ScanService.CallbackActions.JobStatusEvent, IsOneWay = true)]
    Task JobStatusEventAsync(JobStatusEvent request);

    [OperationContract(Action = Xd.ScanService.CallbackActions.JobEndStateEvent, IsOneWay = true)]
    Task JobEndStateEventAsync(JobEndStateEvent request);
}

public class WsWsdScanClient(string uri)
    : ClientBase<IWsWsdScannerService>(new WsdClientBinding(), new EndpointAddress(uri)), IWsWsdScannerService
{
    public Task<CreateScanJobResponse> CreateScanJobAsync(CreateScanJobRequest request)
    {
        return Channel.CreateScanJobAsync(request);
    }

    public Task<RetrieveImageResponse> RetrieveImageAsync(RetrieveImageRequest request)
    {
        return Channel.RetrieveImageAsync(request);
    }

    public Task<CancelJobResponse> CancelJobAsync(CancelJobRequest request)
    {
        return Channel.CancelJobAsync(request);
    }

    public Task<ValidateScanTicketResponse> ValidateScanTicketAsync(ValidateScanTicketRequest request)
    {
        return Channel.ValidateScanTicketAsync(request);
    }

    public Task<GetScannerElementsResponse> GetScannerElementsAsync(GetScannerElementsRequest request)
    {
        return Channel.GetScannerElementsAsync(request);
    }

    public Task<GetJobElementsResponse> GetJobElementsAsync(GetJobElementsRequest request)
    {
        return Channel.GetJobElementsAsync(request);
    }

    public Task<GetActiveJobsResponse> GetActiveJobsAsync(GetActiveJobsRequest request)
    {
        return Channel.GetActiveJobsAsync(request);
    }

    public Task<GetJobHistoryResponse> GetJobHistoryAsync(GetJobHistoryRequest request)
    {
        return Channel.GetJobHistoryAsync(request);
    }
}
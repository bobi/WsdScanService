using System.Xml;
using System.Xml.Serialization;

namespace WsdScanService.Common;

public static class Xd
{
    public static class Addressing200408
    {
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
        public const string Anonymous = "http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous";
        public const string FaultAction = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault";
    }

    public static class MetadataExchange
    {
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex";
    }

    public static class DeviceProfile
    {
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2006/02/devprof";

        public const string DialectAction = Namespace + "/Action";

        public const string DialectThisModel = Namespace + "/ThisModel";
        public const string DialectThisDevice = Namespace + "/ThisDevice";
        public const string DialectRelationship = Namespace + "/Relationship";

        public const string RelationshipTypeHost = Namespace + "/host";
    }

    public static class Transfer
    {
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer";

        public static class Actions
        {
            public const string Get = Namespace + "/Get";
        }

        public static class Reply
        {
            public const string Get = Namespace + "/GetResponse";
        }
    }

    public static class Eventing
    {
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/08/eventing";

        public static class Actions
        {
            public const string Subscribe = Namespace + "/Subscribe";
            public const string Renew = Namespace + "/Renew";
            public const string GetStatus = Namespace + "/GetStatus";
            public const string Unsubscribe = Namespace + "/Unsubscribe";
        }

        public static class Reply
        {
            public const string Subscribe = Namespace + "/SubscribeResponse";
            public const string Renew = Namespace + "/RenewResponse";
            public const string GetStatus = Namespace + "/GetStatusResponse";
            public const string Unsubscribe = Namespace + "/UnsubscribeResponse";
            public const string SubscriptionEnd = Namespace + "/SubscriptionEnd";
        }

        public const string DeliveryModesPush = Namespace + "/DeliveryModes/Push";
    }

    public static class ScanService
    {
        public const string Namespace = "http://schemas.microsoft.com/windows/2006/08/wdp/scan";

        public static class Actions
        {
            public const string CreateScanJob = Namespace + "/CreateScanJob";
            public const string RetrieveImage = Namespace + "/RetrieveImage";
            public const string CancelJob = Namespace + "/CancelJob";
            public const string ValidateScanTicket = Namespace + "/ValidateScanTicket";
            public const string GetScannerElements = Namespace + "/GetScannerElements";
            public const string GetJobElements = Namespace + "/GetJobElements";
            public const string GetActiveJobs = Namespace + "/GetActiveJobs";
            public const string GetJobHistory = Namespace + "/GetJobHistory";
        }

        public static class CallbackActions
        {
            public const string ScanAvailableEvent = Namespace + "/ScanAvailableEvent";
            public const string ScannerElementsChangeEvent = Namespace + "/ScannerElementsChangeEvent";
            public const string ScannerStatusSummaryEvent = Namespace + "/ScannerStatusSummaryEvent";
            public const string ScannerStatusConditionEvent = Namespace + "/ScannerStatusConditionEvent";
            public const string ScannerStatusConditionClear = Namespace + "/ScannerStatusConditionClear";
            public const string JobStatusEvent = Namespace + "/JobStatusEvent";
            public const string JobEndStateEvent = Namespace + "/JobEndStateEvent";
        }

        public static readonly XmlQualifiedName ScannerServiceTypeQName =
            new("ScannerServiceType", Namespace);

        [XmlType(Namespace = Namespace)]
        public enum GetScannerElementsRequestedElements
        {
            ScannerDescription,
            ScannerConfiguration,
            ScannerStatus,
            DefaultScanTicket,
            VendorSection
        }
    }
}
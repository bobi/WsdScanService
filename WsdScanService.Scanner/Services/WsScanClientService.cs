using Microsoft.Extensions.Logging;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Contracts.ScanService;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.Services;

internal class WsScanClientService(
    ILogger<WsScanClientService> logger)
{
    public async Task<ScanJob> CreateScanJobAsync(
        string scanServiceAddress,
        string scanIdentifier,
        string destinationToken,
        ScanTicket scanTicket
    )
    {
        var wsWsdScanClient = WsScannerClient.Create(scanServiceAddress, logger);

        var request = new CreateScanJobRequest
        {
            CreateScanJobRequest1 = new CreateScanJobRequestType
            {
                ScanIdentifier = new String255ExtType { Value = scanIdentifier },
                DestinationToken = new String255ExtType { Value = destinationToken },
                ScanTicket = new ScanTicketType
                {
                    JobDescription = new JobDescriptionType
                        { JobName = new String255ExtType { Value = "Scan Job" } },

                    DocumentParameters = new DocumentParametersType
                    {
                        Format = new DocumentFormatType { Value = scanTicket.Format },
                        CompressionQualityFactor = new CompressionQualityFactorType { Value = scanTicket.Quality },
                        InputSource = new DocumentInputSourceType { Value = scanTicket.InputSource },
                        ContentType = new ContentTypeTicketType { Value = scanTicket.ContentType },
                        InputSize = new DocumentInputSizeType
                            { Item = new BoolExtType { Value = scanTicket.SizeAutoDetect } },
                        ImagesToTransfer = new ImagesToTransferType { Value = scanTicket.ImagesToTransfer },

                        MediaSides = new MediaSidesType
                        {
                            MediaFront = new MediaSideType
                            {
                                Resolution = new ResolutionPairExtType
                                {
                                    Width = new IntOneExtType { Value = scanTicket.Resolution },
                                    Height = new IntOneExtType { Value = scanTicket.Resolution },
                                }
                            }
                        }
                    }
                }
            }
        };

        var response = await wsWsdScanClient.CreateScanJobAsync(request);

        return new ScanJob
        {
            JobId = response.CreateScanJobResponse1.JobId.Value,
            JobToken = response.CreateScanJobResponse1.JobToken.Value,
            ImagesToTransfer = response.CreateScanJobResponse1.DocumentFinalParameters.ImagesToTransfer.Value,
        };
    }

    public async Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob)
    {
        var wsWsdScanClient = WsScannerClient.Create(scanServiceAddress, logger);

        var cancelJobRequest = new CancelJobRequest
        {
            CancelJobRequest1 = new CancelJobRequestType
            {
                JobId = new IntOneExtType { Value = scanJob.JobId },
            }
        };

        await wsWsdScanClient.CancelJobAsync(cancelJobRequest);
    }


    public async Task<byte[]?> RetrieveImage(string scanServiceAddress, ScanJob scanJob)
    {
        var wsWsdScanClient = WsScannerClient.Create(scanServiceAddress, logger);

        var retrieveRequest = new RetrieveImageRequest
        {
            RetrieveImageRequest1 = new RetrieveImageRequestType
            {
                JobId = new IntOneExtType { Value = scanJob.JobId },
                JobToken = new String255ExtType { Value = scanJob.JobToken }
            }
        };

        var retrieveResponse = await wsWsdScanClient.RetrieveImageAsync(retrieveRequest);

        return retrieveResponse.RetrieveImageResponse1.ScanData.Value;
    }

    public async Task GetActiveJobsAsync(string scanServiceAddress)
    {
        var wsWsdScanClient = WsScannerClient.Create(scanServiceAddress, logger);

        var request = new GetActiveJobsRequest()
        {
            GetActiveJobsRequest1 = new GetActiveJobsRequestType()
        };

        await wsWsdScanClient.GetActiveJobsAsync(request);
    }

    public async Task GetJobHistoryAsync(string scanServiceAddress)
    {
        var wsWsdScanClient = WsScannerClient.Create(scanServiceAddress, logger);

        var request = new GetJobHistoryRequest()
        {
            GetJobHistoryRequest1 = new GetJobHistoryRequestType()
        };

        await wsWsdScanClient.GetJobHistoryAsync(request);
    }
}
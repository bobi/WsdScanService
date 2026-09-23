using Microsoft.Extensions.Hosting;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Scanner.Contracts;

public interface ISaneScanner : IHostedService
{
    public Task<ScanJob> CreateScanJobAsync(ScanTicket scanTicket, ImageConverterConfiguration? imageConverter = null);

    public Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob);

    public Task<byte[]?> RetrieveImage(string scanServiceAddress, ScanJob scanJob);
}
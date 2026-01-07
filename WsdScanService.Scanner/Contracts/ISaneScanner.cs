using Microsoft.Extensions.Hosting;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Scanner.Contracts;

public interface ISaneScanner : IHostedService
{
    public Task<ScanJob> CreateScanJobAsync(
        string scanServiceAddress,
        string scanIdentifier,
        string destinationToken,
        ScanTicket scanTicket
    );

    public Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob);

    public Task<byte[]?> RetrieveImage(string scanServiceAddress, ScanJob scanJob);
}
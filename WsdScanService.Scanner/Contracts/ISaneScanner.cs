using Microsoft.Extensions.Hosting;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Scanner.Contracts;

public interface ISaneScanner : IHostedService
{
    public Task<ScanJob> CreateScanJobAsync(ScanTicket scanTicket);

    public Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob);

    // Returns path to a temp file with the scanned image; caller owns and must delete it
    public Task<string> RetrieveImage(string scanServiceAddress, ScanJob scanJob);
}
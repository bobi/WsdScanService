using Microsoft.Extensions.Hosting;

namespace WsdScanService.Contracts.Scanner;

public interface IScanJobManager : IHostedService
{
    public void StartNewJob(string deviceId, string clientContext, string scanIdentifier, string? inputSource);
}
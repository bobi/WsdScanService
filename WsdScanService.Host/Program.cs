using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Discovery;
using WsdScanService.Host;
using WsdScanService.Scanner;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Configuration.AddEnvironmentVariables("WS_SCAN_");

builder.Services.AddWsScanServiceHostServices(builder.Configuration);
builder.Services.AddWsdDiscoveryServices(builder.Configuration);
builder.Services.AddWsWsdScannerServices(builder.Configuration);

// builder.Services.AddSingleton<TestService>();
// builder.Services.AddHostedService<BackgroundServiceStarter<TestService>>();

var app = builder.Build();

app.ConfigureWsWsdScannerServices();
var wsdScanServiceHostConfiguration = app.Services.GetRequiredService<IOptions<WsdScanServiceHostConfiguration>>();

var hostIp = wsdScanServiceHostConfiguration.Value.Ip;
var listenPort = wsdScanServiceHostConfiguration.Value.Port;

app.Logger.LogInformation($"Using IP: {hostIp}, Port: {listenPort}");

app.Run($"http://{hostIp}:{listenPort}");
using Microsoft.Extensions.Options;
using System.Text.Json;
using WsdScanService.Common.Configuration;
using WsdScanService.Discovery;
using WsdScanService.Host;
using WsdScanService.Scanner;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Configuration.AddEnvironmentVariables("WS_SCAN_");

builder.Configuration.AddJsonFile("custom-settings.json", optional: true, reloadOnChange: false);

builder.Services.AddHostServices(builder.Configuration);
builder.Services.AddWsdDiscoveryServices(builder.Configuration);
builder.Services.AddScannerServices(builder.Configuration);

var app = builder.Build();

app.ConfigureScannerServices();
var wsdScanServiceHostConfiguration = app.Services.GetRequiredService<IOptions<ScanServiceConfiguration>>();


var config = wsdScanServiceHostConfiguration.Value;

app.Logger.LogInformation(
    "Loaded Configuration:\n{Config}",
    JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true })
);

var hostIp = config.Ip;
var listenPort = config.Port;

app.Logger.LogInformation($"Using IP: {hostIp}, Port: {listenPort}");

app.Run($"http://{hostIp}:{listenPort}");
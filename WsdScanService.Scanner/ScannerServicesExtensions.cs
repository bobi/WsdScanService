using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Scanner.Contracts;
using WsdScanService.Scanner.Services;
using WsdScanService.Scanner.Utils;
using WsdScanService.Scanner.Wcf;

namespace WsdScanService.Scanner;

public static class ScannerServicesExtensions
{
    public static IServiceCollection AddScannerServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        services.AddServiceModelServices().AddServiceModelMetadata();

        services.AddSingleton<WsEventingClientService>();
        services.AddSingleton<WsTransferClientService>();
        services.AddSingleton<WsScanClientService>();

        // services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>(); // Optional for tracing
        // services.AddSingleton<IServiceBehavior, ServiceDebugBehavior>(); // Optional for tracing
        services.AddSingleton<IServiceBehavior, LoggingServiceBehaviour>(); // Optional for tracing

        services.AddScoped<WsScanCallbackService>();

        services.AddSingleton<IWsScanner, WsScanner>();

        services.AddSingleton<ISaneScanner, SaneScanner>()
            .AddHostedService(provider => provider.GetRequiredService<ISaneScanner>());

        return services;
    }

    public static IApplicationBuilder ConfigureScannerServices(this IApplicationBuilder app)
    {
        var wsdScanServiceHostConfiguration =
            app.ApplicationServices.GetRequiredService<IOptions<ScanServiceConfiguration>>();

        var soapEndpointPath = wsdScanServiceHostConfiguration.Value.ScanEndpoint;

        app.UseServiceModel(builder =>
            {
                builder.AddService<WsScanCallbackService>(_ => { })
                    .AddServiceEndpoint<WsScanCallbackService, IWsScannerCallback>(
                        new WsdServerBinding(),
                        soapEndpointPath
                    );

                var serviceMetadataBehavior =
                    app.ApplicationServices.GetRequiredService<ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
            }
        );

        return app;
    }
}
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts;
using WsdScanService.Scanner.CoreWcf;
using WsdScanService.Scanner.Services;

namespace WsdScanService.Scanner;

public static class WsWsdScannerServicesExtensions
{
    public static IServiceCollection AddWsWsdScannerServices(this IServiceCollection services,
        IConfiguration config)
    {

        services.AddOptions<Configuration.Configuration>()
            .Bind(config.GetSection(Configuration.Configuration.WsScanner));

        services.AddServiceModelServices().AddServiceModelMetadata();
        services.AddSingleton<WsWsdScanClientService>();

        // services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>(); // Optional for tracing
        // services.AddSingleton<IServiceBehavior, ServiceDebugBehavior>(); // Optional for tracing
        services.AddSingleton<IServiceBehavior, LoggingServiceBehaviour>(); // Optional for tracing
        services.AddScoped<WsWsdScanCallbackService>();
        
        services.AddSingleton<ScanJobManager>();
        services.AddHostedService(provider => provider.GetRequiredService<ScanJobManager>());

        return services;
    }

    public static IApplicationBuilder ConfigureWsWsdScannerServices(this IApplicationBuilder app)
    {
        var wsdScanServiceHostConfiguration =
            app.ApplicationServices.GetRequiredService<IOptions<WsdScanServiceHostConfiguration>>();

        var soapEndpointPath = wsdScanServiceHostConfiguration.Value.ScanEndpoint;

        app.UseServiceModel(builder =>
        {
            builder.AddService<WsWsdScanCallbackService>(_ => { })
                .AddServiceEndpoint<WsWsdScanCallbackService, IWsWsdScannerServiceCallback>(
                    new WsdServerBinding(),
                    soapEndpointPath
                );

            var serviceMetadataBehavior =
                app.ApplicationServices.GetRequiredService<ServiceMetadataBehavior>();
            serviceMetadataBehavior.HttpGetEnabled = true;
        });

        return app;
    }
}
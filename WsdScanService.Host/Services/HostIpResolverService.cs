using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WsdScanService.Host.Services;

public class HostIpResolverService(ILogger<HostIpResolverService> logger)
{
    public string GetHostIpAddress()
    {
        var ipAddr = GetLocalIpAddressByHostName();

        if (string.IsNullOrEmpty(ipAddr))
        {
            ipAddr = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                )
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .FirstOrDefault(GetLoopbackIpAddress());
        }

        if (string.IsNullOrEmpty(ipAddr))
        {
            ipAddr = IPAddress.Loopback.ToString();
        }

        return ipAddr;
    }

    private static string GetLoopbackIpAddress()
    {
        var ipAddress = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n is
                { OperationalStatus: OperationalStatus.Up, NetworkInterfaceType: NetworkInterfaceType.Loopback }
            )
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => a.Address.ToString())
            .First();

        return ipAddress;
    }

    private string? GetLocalIpAddressByHostName()
    {
        try
        {
            var host = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var ip in host)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !ip.Equals(IPAddress.Loopback))
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e.Message);
        }

        return null;
    }
}
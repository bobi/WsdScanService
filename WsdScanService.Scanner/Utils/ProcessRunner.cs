using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Scanner.Utils;

public class InvalidExitCodeException(string? message) : Exception(message);

public static class ProcessRunner
{
    // Runs the process, logging its output; kills it when the timeout is exceeded and throws on non-zero exit code
    public static async Task RunAsync(ProcessStartInfo info, string name, TimeSpan timeout, ILogger logger)
    {
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.UseShellExecute = false;

        logger.LogDebug("Running {Name}: {FileName} {Arguments}", name, info.FileName, string.Join(" ", info.ArgumentList));

        using var process = new Process();

        process.StartInfo = info;

        var lockObject = new object();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (lockObject)
                {
                    logger.LogInformation("STDOUT: {EData}", e.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (lockObject)
                {
                    logger.LogError("STDERR: {EData}", e.Data);
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using (var cts = new CancellationTokenSource(timeout))
        {
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogError("{Name} did not exit within {Timeout}, killing process", name, timeout);

                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();

                throw new InvalidExitCodeException($"{name} timed out after {timeout}");
            }
        }

        logger.LogDebug("{Name} Exit code: {ProcessExitCode}", name, process.ExitCode);

        if (process.ExitCode != 0)
        {
            throw new InvalidExitCodeException($"{name} exit code: {process.ExitCode}");
        }
    }

    public static string ReplaceNamedParameters(string template, Dictionary<string, string> parameters)
    {
        return parameters.Aggregate(template, (current, kvp) => current.Replace("{" + kvp.Key + "}", kvp.Value));
    }
}

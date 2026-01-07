namespace WsdScanService.Host.Utils;

public static class FileUtils
{
    public static async Task WriteUniqueFileWithSuffix(string initialPath, byte[] content,
        CancellationToken cancellationToken)
    {
        var uniquePath = GetUniqueFilePath(initialPath);

        await File.WriteAllBytesAsync(uniquePath, content, cancellationToken);
    }

    private static string GetUniqueFilePath(string initialPath)
    {
        var directory = Path.GetDirectoryName(initialPath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(initialPath);
        var extension = Path.GetExtension(initialPath);
        var uniquePath = initialPath;
        var counter = 1;

        if (directory == null || fileNameWithoutExtension == null || extension == null)
        {
            throw new ArgumentException("Invalid file path", nameof(initialPath));
        }

        while (File.Exists(uniquePath))
        {
            var newFileName = $"{fileNameWithoutExtension}-{counter}{extension}";
            uniquePath = Path.Combine(directory, newFileName);
            counter++;
        }

        return uniquePath;
    }
}
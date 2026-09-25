namespace WsdScanService.Host.Utils;

public static class FileUtils
{
    // Moves sourcePath to initialPath, adding a -N suffix if taken; falls back to copy+delete across filesystems
    public static string MoveToUniqueFile(string sourcePath, string initialPath)
    {
        var uniquePath = GetUniqueFilePath(initialPath);

        File.Move(sourcePath, uniquePath, overwrite: false);

        return uniquePath;
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
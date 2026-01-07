namespace WsdScanService.Contracts.Scanner.Entities;

public class ScanTicket
{
    public static readonly ScanTicket DefaultScanTicket = new();
    
    public ScanTicket()
    {
    }

    public ScanTicket(ScanTicket scanTicket)
    {
        Format = scanTicket.Format;
        Quality = scanTicket.Quality;
        InputSource = scanTicket.InputSource;
        ContentType = scanTicket.ContentType;
        SizeAutoDetect = scanTicket.SizeAutoDetect;
        ImagesToTransfer = scanTicket.ImagesToTransfer;
        Resolution = scanTicket.Resolution;
    }

    public string Format { get; init; } = "jfif";
    
    public int Quality { get; init; } = 100;
    
    public string InputSource { get; init; } = "Platen";
    
    public string ContentType { get; init; } = "Photo";
    
    public bool SizeAutoDetect { get; init; } = true;
    
    public int ImagesToTransfer { get; init; } = 1;
    
    public int Resolution { get; init; } = 300;
}
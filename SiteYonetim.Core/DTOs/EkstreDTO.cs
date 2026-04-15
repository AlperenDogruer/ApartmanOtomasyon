namespace SiteYonetim.Core.DTOs;

public class EkstreDTO
{
    public string Donem { get; set; } = "";
    public string AidatTipi { get; set; } = "";
    public decimal TahakkukTutar { get; set; }
    public decimal OdenenTutar { get; set; }
    public decimal KalanTutar { get; set; }
    public decimal GecikmeTazminati { get; set; }
    public string? OdemeTarihi { get; set; }
}

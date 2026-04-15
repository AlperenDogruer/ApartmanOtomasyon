using SiteYonetim.Core.Enums;

namespace SiteYonetim.Data.Entities;

public class AidatTahsilat
{
    public int Id { get; set; }
    public int TahakkukId { get; set; }
    public decimal Tutar { get; set; }
    public DateTime OdemeTarihi { get; set; }
    public OdemeTipi OdemeTipi { get; set; }
    public string? Aciklama { get; set; }
    public AidatTahakkuk Tahakkuk { get; set; } = null!;
}

using SiteYonetim.Core.Enums;

namespace SiteYonetim.Data.Entities;

public class GelirKalemi
{
    public int Id { get; set; }
    public string Aciklama { get; set; } = "";
    public decimal Tutar { get; set; }
    public DateTime Tarih { get; set; }
    public string Kategori { get; set; } = "Diğer";
    public OdemeTipi OdemeTipi { get; set; }
    public string? BelgeNo { get; set; }
}

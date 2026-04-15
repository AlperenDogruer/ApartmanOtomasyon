namespace SiteYonetim.Data.Entities;

public class AidatTipi
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public bool Aktif { get; set; } = true;
    public ICollection<AidatTahakkuk> Tahakkuklar { get; set; } = new List<AidatTahakkuk>();
}

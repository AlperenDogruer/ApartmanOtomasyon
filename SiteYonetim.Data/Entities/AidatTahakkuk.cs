using SiteYonetim.Core.Enums;

namespace SiteYonetim.Data.Entities;

public class AidatTahakkuk
{
    public int Id { get; set; }
    public int DaireId { get; set; }
    public int AidatTipiId { get; set; }
    public int Yil { get; set; }
    public int Ay { get; set; }
    public decimal Tutar { get; set; }
    public decimal OdenenTutar { get; set; } = 0;
    public decimal GecikmeTazminati { get; set; } = 0;
    public TahakkukDurumu Durum { get; set; } = TahakkukDurumu.Odenmemis;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public DateTime? SonOdemeTarihi { get; set; }
    public Daire Daire { get; set; } = null!;
    public AidatTipi AidatTipi { get; set; } = null!;
    public ICollection<AidatTahsilat> Tahsilatlar { get; set; } = new List<AidatTahsilat>();
}

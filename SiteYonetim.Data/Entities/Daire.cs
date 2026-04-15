using SiteYonetim.Core.Enums;

namespace SiteYonetim.Data.Entities;

public class Daire
{
    public int Id { get; set; }
    public int BlokId { get; set; }
    public string DaireNo { get; set; } = "";
    public int Kat { get; set; }
    public float ArsakPayi { get; set; }
    public DaireTipi Tip { get; set; }
    public Blok Blok { get; set; } = null!;
    public ICollection<Sakin> Sakinler { get; set; } = new List<Sakin>();
    public ICollection<AidatTahakkuk> Tahakkuklar { get; set; } = new List<AidatTahakkuk>();
}

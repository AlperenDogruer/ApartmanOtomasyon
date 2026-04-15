using SiteYonetim.Core.Enums;

namespace SiteYonetim.Data.Entities;

public class Sakin
{
    public int Id { get; set; }
    public int DaireId { get; set; }
    public string Ad { get; set; } = "";
    public string Soyad { get; set; } = "";
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? TcKimlik { get; set; }
    public SakinTipi Tip { get; set; }
    public DateTime GirisTarihi { get; set; }
    public DateTime? CikisTarihi { get; set; }
    public bool Aktif { get; set; } = true;
    public Daire Daire { get; set; } = null!;
}

namespace SiteYonetim.Data.Entities;

public class Blok
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public ICollection<Daire> Daireler { get; set; } = new List<Daire>();
}

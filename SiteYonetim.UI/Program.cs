using System.Globalization;
using SiteYonetim.Data;

namespace SiteYonetim.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Veritabanını oluştur/güncelle
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
        }

        // İlk kurulum kontrolü
        bool ilkKurulum;
        using (var db = new AppDbContext())
        {
            ilkKurulum = !db.SiteAyarlar.Any();
        }

        if (ilkKurulum)
        {
            var wizard = new Dialogs.KurulumSihirbaziForm();
            if (wizard.ShowDialog() != DialogResult.OK)
                return;
        }

        Application.Run(new MainForm());
    }
}

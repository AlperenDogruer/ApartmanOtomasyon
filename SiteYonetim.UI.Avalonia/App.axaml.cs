using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SiteYonetim.Data;
using SiteYonetim.UI.Avalonia.ViewModels;
using SiteYonetim.UI.Avalonia.Views;
using System.Linq;

namespace SiteYonetim.UI.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Ensure database exists
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            // Check first-run
            bool ilkKurulum;
            using (var db = new AppDbContext())
            {
                ilkKurulum = !db.SiteAyarlar.Any();
            }

            if (ilkKurulum)
            {
                var wizardVm = new SetupWizardViewModel();
                var wizard = new SetupWizardWindow { DataContext = wizardVm };
                wizardVm.RequestClose += (_, success) =>
                {
                    wizard.Close();
                    if (success)
                    {
                        desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
                        desktop.MainWindow.Show();
                    }
                    else
                    {
                        desktop.Shutdown();
                    }
                };
                desktop.MainWindow = wizard;
            }
            else
            {
                desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}

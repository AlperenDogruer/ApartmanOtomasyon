using SiteYonetim.Business.Services;
using SiteYonetim.Data;
using SiteYonetim.UI.Controls;

namespace SiteYonetim.UI;

public class MainForm : Form
{
    private Panel panelSidebar = null!;
    private Panel panelTopBar = null!;
    private Panel panelContent = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblKasa = null!;
    private ToolStripStatusLabel lblBorclu = null!;
    private Label lblSiteAdi = null!;
    private Label lblTarih = null!;

    private Button? _activeButton;

    public MainForm()
    {
        InitializeComponent();
        LoadSidebar();
        MenuTiklandi("Dashboard", typeof(DashboardControl));
    }

    private void InitializeComponent()
    {
        this.Text = "Site Yönetim Otomasyon";
        this.MinimumSize = new Size(1024, 768);
        this.Size = new Size(1280, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 244, 248);

        // Sidebar
        panelSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Color.FromArgb(30, 58, 95),
            Padding = new Padding(0, 10, 0, 0)
        };

        // Logo area
        var lblLogo = new Label
        {
            Text = "🏢 Site Yönetim",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 5, 0, 5)
        };
        panelSidebar.Controls.Add(lblLogo);

        var separator = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(46, 134, 171) };
        panelSidebar.Controls.Add(separator);

        // Top Bar
        panelTopBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White,
            Padding = new Padding(15, 0, 15, 0)
        };

        lblSiteAdi = new Label
        {
            Text = GetSiteAdi(),
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Dock = DockStyle.Left,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 12, 0, 0)
        };
        panelTopBar.Controls.Add(lblSiteAdi);

        lblTarih = new Label
        {
            Text = DateTime.Now.ToString("dd MMMM yyyy, dddd"),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            Dock = DockStyle.Right,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 15, 0, 0)
        };
        panelTopBar.Controls.Add(lblTarih);

        var topSeparator = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Color.FromArgb(46, 134, 171) };
        panelTopBar.Controls.Add(topSeparator);

        // Status Strip
        statusStrip = new StatusStrip { BackColor = Color.FromArgb(30, 58, 95) };
        lblKasa = new ToolStripStatusLabel("Kasa: ₺0,00")
        {
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        lblBorclu = new ToolStripStatusLabel("Bu ay borçlu: 0 daire")
        {
            ForeColor = Color.FromArgb(243, 156, 18),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblKasa, lblBorclu });

        // Content Panel
        panelContent = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 244, 248),
            Padding = new Padding(15)
        };

        // Add controls in correct order
        this.Controls.Add(panelContent);
        this.Controls.Add(panelTopBar);
        this.Controls.Add(panelSidebar);
        this.Controls.Add(statusStrip);

        UpdateStatusBar();
    }

    private void LoadSidebar()
    {
        var menuItems = new (string icon, string text, Type controlType)[]
        {
            ("🏠", "Dashboard", typeof(DashboardControl)),
            ("🏢", "Daire & Sakin", typeof(DaireControl)),
            ("👤", "Sakinler", typeof(SakinControl)),
            ("💰", "Aidat İşlemleri", typeof(AidatControl)),
            ("📊", "Gelir & Gider", typeof(GelirGiderControl)),
            ("📋", "Raporlar", typeof(RaporlarControl)),
            ("⚙️", "Ayarlar", typeof(AyarlarControl))
        };

        int y = 65;
        foreach (var (icon, text, controlType) in menuItems)
        {
            var btn = new Button
            {
                Text = $"  {icon}  {text}",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 58, 95),
                Size = new Size(220, 45),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = controlType
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 134, 171);
            btn.Click += (s, e) => MenuTiklandi(text, controlType, btn);
            panelSidebar.Controls.Add(btn);
            y += 45;
        }
    }

    private void MenuTiklandi(string text, Type controlType, Button? btn = null)
    {
        if (_activeButton != null)
            _activeButton.BackColor = Color.FromArgb(30, 58, 95);

        if (btn != null)
        {
            btn.BackColor = Color.FromArgb(46, 134, 171);
            _activeButton = btn;
        }
        else
        {
            // first load - find the button
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Button b && b.Tag as Type == controlType)
                {
                    b.BackColor = Color.FromArgb(46, 134, 171);
                    _activeButton = b;
                    break;
                }
            }
        }

        panelContent.Controls.Clear();
        var uc = (UserControl)Activator.CreateInstance(controlType)!;
        uc.Dock = DockStyle.Fill;

        if (uc is IRefreshable refreshable)
            refreshable.OnDataChanged += () => UpdateStatusBar();

        panelContent.Controls.Add(uc);
    }

    public void UpdateStatusBar()
    {
        try
        {
            var service = new GelirGiderService();
            var now = DateTime.Now;
            var ayBas = new DateTime(now.Year, now.Month, 1);
            var ayBit = ayBas.AddMonths(1).AddDays(-1);
            var (gelir, gider, net) = service.KasaDurumu(new DateTime(2000, 1, 1), DateTime.Now);

            var ozet = service.AylikOzet(now.Year, now.Month);

            lblKasa.Text = $"Kasa Bakiyesi: {net:₺#,##0.00}";
            lblKasa.ForeColor = net >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(231, 76, 60);
            lblBorclu.Text = $"Bu ay borçlu: {ozet.BorcluDaireSayisi} daire";
        }
        catch { }
    }

    private string GetSiteAdi()
    {
        try
        {
            using var db = new AppDbContext();
            return db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SiteAdi")?.Deger ?? "Site Yönetim";
        }
        catch { return "Site Yönetim"; }
    }
}

public interface IRefreshable
{
    event Action? OnDataChanged;
}

using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Dialogs;

public class KurulumSihirbaziForm : Form
{
    private Panel panelStep1 = null!;
    private Panel panelStep2 = null!;
    private TextBox txtSiteAdi = null!;
    private TextBox txtYoneticiAdi = null!;
    private Button btnIleri = null!;
    private Button btnGeri = null!;
    private Button btnTamamla = null!;
    private Label lblBaslik = null!;
    private Label lblAdim = null!;
    private int _currentStep = 1;

    public KurulumSihirbaziForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Kurulum Sihirbazi";
        this.Size = new Size(500, 400);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        // Header panel
        var panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(30, 58, 95)
        };

        lblBaslik = new Label
        {
            Text = "Site Yonetim Kurulum Sihirbazi",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 10),
            AutoSize = true
        };

        lblAdim = new Label
        {
            Text = "Adim 1/2 - Site Bilgileri",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(46, 134, 171),
            Location = new Point(20, 42),
            AutoSize = true
        };

        panelHeader.Controls.Add(lblBaslik);
        panelHeader.Controls.Add(lblAdim);

        // Step 1 - Site Name
        panelStep1 = new Panel
        {
            Location = new Point(0, 70),
            Size = new Size(500, 230)
        };

        var lblSiteAdiLabel = new Label
        {
            Text = "Site / Apartman Adi:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(40, 30),
            AutoSize = true
        };

        txtSiteAdi = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Location = new Point(40, 60),
            Size = new Size(400, 30),
            PlaceholderText = "Ornegin: Yesil Vadi Sitesi"
        };

        var lblSiteInfo = new Label
        {
            Text = "Site veya apartmaninizin adini giriniz.",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(40, 95),
            AutoSize = true
        };

        panelStep1.Controls.AddRange(new Control[] { lblSiteAdiLabel, txtSiteAdi, lblSiteInfo });

        // Step 2 - Manager Name
        panelStep2 = new Panel
        {
            Location = new Point(0, 70),
            Size = new Size(500, 230),
            Visible = false
        };

        var lblYoneticiLabel = new Label
        {
            Text = "Yonetici Adi Soyadi:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(40, 30),
            AutoSize = true
        };

        txtYoneticiAdi = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Location = new Point(40, 60),
            Size = new Size(400, 30),
            PlaceholderText = "Ornegin: Ahmet Yilmaz"
        };

        var lblYoneticiInfo = new Label
        {
            Text = "Site yoneticisinin ad ve soyadini giriniz.",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(40, 95),
            AutoSize = true
        };

        panelStep2.Controls.AddRange(new Control[] { lblYoneticiLabel, txtYoneticiAdi, lblYoneticiInfo });

        // Buttons
        btnGeri = new Button
        {
            Text = "< Geri",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 38),
            Location = new Point(150, 310),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 58, 95),
            Visible = false,
            Cursor = Cursors.Hand
        };
        btnGeri.FlatAppearance.BorderColor = Color.FromArgb(30, 58, 95);
        btnGeri.Click += BtnGeri_Click;

        btnIleri = new Button
        {
            Text = "Ileri >",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 38),
            Location = new Point(260, 310),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnIleri.FlatAppearance.BorderSize = 0;
        btnIleri.Click += BtnIleri_Click;

        btnTamamla = new Button
        {
            Text = "Tamamla",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 38),
            Location = new Point(360, 310),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            Visible = false,
            Cursor = Cursors.Hand
        };
        btnTamamla.FlatAppearance.BorderSize = 0;
        btnTamamla.Click += BtnTamamla_Click;

        this.Controls.Add(panelHeader);
        this.Controls.Add(panelStep1);
        this.Controls.Add(panelStep2);
        this.Controls.Add(btnGeri);
        this.Controls.Add(btnIleri);
        this.Controls.Add(btnTamamla);
    }

    private void BtnIleri_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSiteAdi.Text))
        {
            MessageBox.Show("Lutfen site adini giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _currentStep = 2;
        panelStep1.Visible = false;
        panelStep2.Visible = true;
        btnGeri.Visible = true;
        btnIleri.Visible = false;
        btnTamamla.Visible = true;
        lblAdim.Text = "Adim 2/2 - Yonetici Bilgileri";
    }

    private void BtnGeri_Click(object? sender, EventArgs e)
    {
        _currentStep = 1;
        panelStep1.Visible = true;
        panelStep2.Visible = false;
        btnGeri.Visible = false;
        btnIleri.Visible = true;
        btnTamamla.Visible = false;
        lblAdim.Text = "Adim 1/2 - Site Bilgileri";
    }

    private void BtnTamamla_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtYoneticiAdi.Text))
        {
            MessageBox.Show("Lutfen yonetici adini giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            // Save site settings
            db.SiteAyarlar.Add(new SiteAyar { Anahtar = "SiteAdi", Deger = txtSiteAdi.Text.Trim() });
            db.SiteAyarlar.Add(new SiteAyar { Anahtar = "YoneticiAdi", Deger = txtYoneticiAdi.Text.Trim() });
            db.SiteAyarlar.Add(new SiteAyar { Anahtar = "KurulumTamamlandi", Deger = "true" });

            // Create 2 blocks
            var blokA = new Blok { Ad = "A Blok" };
            var blokB = new Blok { Ad = "B Blok" };
            db.Bloklar.AddRange(blokA, blokB);
            db.SaveChanges();

            // Create 6 apartments: A Blok 1-3, B Blok 1-3
            for (int i = 1; i <= 3; i++)
            {
                db.Daireler.Add(new Daire
                {
                    BlokId = blokA.Id,
                    DaireNo = i.ToString(),
                    Kat = i,
                    ArsakPayi = 100,
                    Tip = DaireTipi.Daire
                });
                db.Daireler.Add(new Daire
                {
                    BlokId = blokB.Id,
                    DaireNo = i.ToString(),
                    Kat = i,
                    ArsakPayi = 100,
                    Tip = DaireTipi.Daire
                });
            }

            // Create 1 aidat type
            db.AidatTipleri.Add(new AidatTipi { Ad = "Isletme Aidati", Aktif = true });

            db.SaveChanges();

            MessageBox.Show(
                "Kurulum tamamlandi! Sakin ve aidat bilgilerini girebilirsiniz.",
                "Basarili",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kurulum sirasinda bir hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

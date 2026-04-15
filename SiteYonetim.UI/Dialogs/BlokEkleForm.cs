using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Dialogs;

public class BlokEkleForm : Form
{
    private TextBox txtAd = null!;
    private TextBox txtAciklama = null!;
    private Button btnKaydet = null!;
    private Button btnIptal = null!;

    private readonly Blok? _mevcutBlok;

    public string BlokAd => txtAd.Text.Trim();
    public string BlokAciklama => txtAciklama.Text.Trim();

    public BlokEkleForm(Blok? mevcutBlok = null)
    {
        _mevcutBlok = mevcutBlok;
        InitializeComponent();

        if (_mevcutBlok != null)
        {
            this.Text = "Blok Duzenle";
            txtAd.Text = _mevcutBlok.Ad;
            txtAciklama.Text = _mevcutBlok.Aciklama ?? "";
        }
    }

    private void InitializeComponent()
    {
        this.Text = "Yeni Blok Ekle";
        this.Size = new Size(420, 280);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        // Header
        var panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(30, 58, 95)
        };

        var lblBaslik = new Label
        {
            Text = _mevcutBlok != null ? "Blok Duzenle" : "Yeni Blok Ekle",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblBaslik);

        // Ad
        var lblAd = new Label
        {
            Text = "Blok Adi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(20, 65),
            AutoSize = true
        };

        txtAd = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Location = new Point(20, 90),
            Size = new Size(360, 30)
        };

        // Aciklama
        var lblAciklama = new Label
        {
            Text = "Aciklama:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(20, 125),
            AutoSize = true
        };

        txtAciklama = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Location = new Point(20, 150),
            Size = new Size(360, 30)
        };

        // Buttons
        btnKaydet = new Button
        {
            Text = "Kaydet",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 36),
            Location = new Point(170, 195),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnKaydet.FlatAppearance.BorderSize = 0;
        btnKaydet.Click += BtnKaydet_Click;

        btnIptal = new Button
        {
            Text = "Iptal",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 36),
            Location = new Point(280, 195),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 58, 95),
            Cursor = Cursors.Hand
        };
        btnIptal.FlatAppearance.BorderColor = Color.FromArgb(30, 58, 95);
        btnIptal.Click += (s, e) =>
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        this.Controls.Add(panelHeader);
        this.Controls.Add(lblAd);
        this.Controls.Add(txtAd);
        this.Controls.Add(lblAciklama);
        this.Controls.Add(txtAciklama);
        this.Controls.Add(btnKaydet);
        this.Controls.Add(btnIptal);

        this.AcceptButton = btnKaydet;
        this.CancelButton = btnIptal;
    }

    private void BtnKaydet_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtAd.Text))
        {
            MessageBox.Show("Lutfen blok adini giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}

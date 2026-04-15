using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Dialogs;

public class SakinEkleForm : Form
{
    private TextBox txtAd = null!;
    private TextBox txtSoyad = null!;
    private TextBox txtTcKimlik = null!;
    private TextBox txtTelefon = null!;
    private TextBox txtEmail = null!;
    private ComboBox cmbDaire = null!;
    private ComboBox cmbTip = null!;
    private DateTimePicker dtpGirisTarihi = null!;
    private Button btnKaydet = null!;
    private Button btnIptal = null!;

    private readonly Sakin? _mevcutSakin;
    private List<Daire> _daireler = new();

    public string SakinAd => txtAd.Text.Trim();
    public string SakinSoyad => txtSoyad.Text.Trim();
    public string? TcKimlik => string.IsNullOrWhiteSpace(txtTcKimlik.Text) ? null : txtTcKimlik.Text.Trim();
    public string? Telefon => string.IsNullOrWhiteSpace(txtTelefon.Text) ? null : txtTelefon.Text.Trim();
    public string? Email => string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
    public int SeciliDaireId => (_daireler.Count > 0 && cmbDaire.SelectedIndex >= 0)
        ? _daireler[cmbDaire.SelectedIndex].Id : 0;
    public SakinTipi SakinTipiSecim => (SakinTipi)cmbTip.SelectedItem!;
    public DateTime GirisTarihi => dtpGirisTarihi.Value;

    public SakinEkleForm(Sakin? mevcutSakin = null)
    {
        _mevcutSakin = mevcutSakin;
        InitializeComponent();
        LoadDaireler();

        if (_mevcutSakin != null)
        {
            this.Text = "Sakin Duzenle";
            txtAd.Text = _mevcutSakin.Ad;
            txtSoyad.Text = _mevcutSakin.Soyad;
            txtTcKimlik.Text = _mevcutSakin.TcKimlik ?? "";
            txtTelefon.Text = _mevcutSakin.Telefon ?? "";
            txtEmail.Text = _mevcutSakin.Email ?? "";
            var daireIndex = _daireler.FindIndex(d => d.Id == _mevcutSakin.DaireId);
            if (daireIndex >= 0) cmbDaire.SelectedIndex = daireIndex;
            cmbTip.SelectedItem = _mevcutSakin.Tip;
            dtpGirisTarihi.Value = _mevcutSakin.GirisTarihi;
        }
    }

    private void InitializeComponent()
    {
        this.Text = "Yeni Sakin Ekle";
        this.Size = new Size(480, 530);
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
            Text = _mevcutSakin != null ? "Sakin Duzenle" : "Yeni Sakin Ekle",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblBaslik);

        int y = 65;
        int labelX = 20;
        int inputX = 150;
        int inputWidth = 280;

        // Ad
        var lblAd = new Label
        {
            Text = "Ad:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        txtAd = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28)
        };
        y += 38;

        // Soyad
        var lblSoyad = new Label
        {
            Text = "Soyad:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        txtSoyad = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28)
        };
        y += 38;

        // TC Kimlik
        var lblTc = new Label
        {
            Text = "TC Kimlik:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        txtTcKimlik = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            MaxLength = 11
        };
        y += 38;

        // Telefon
        var lblTelefon = new Label
        {
            Text = "Telefon:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        txtTelefon = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28)
        };
        y += 38;

        // Email
        var lblEmail = new Label
        {
            Text = "Email:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        txtEmail = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28)
        };
        y += 38;

        // Daire
        var lblDaire = new Label
        {
            Text = "Daire:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        cmbDaire = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        y += 38;

        // Tip
        var lblTip = new Label
        {
            Text = "Sakin Tipi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        cmbTip = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbTip.Items.AddRange(Enum.GetValues<SakinTipi>().Cast<object>().ToArray());
        cmbTip.SelectedIndex = 0;
        y += 38;

        // Giris Tarihi
        var lblGiris = new Label
        {
            Text = "Giris Tarihi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        dtpGirisTarihi = new DateTimePicker
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today
        };
        y += 50;

        // Buttons
        btnKaydet = new Button
        {
            Text = "Kaydet",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 36),
            Location = new Point(170, y),
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
            Location = new Point(280, y),
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
        this.Controls.Add(lblSoyad);
        this.Controls.Add(txtSoyad);
        this.Controls.Add(lblTc);
        this.Controls.Add(txtTcKimlik);
        this.Controls.Add(lblTelefon);
        this.Controls.Add(txtTelefon);
        this.Controls.Add(lblEmail);
        this.Controls.Add(txtEmail);
        this.Controls.Add(lblDaire);
        this.Controls.Add(cmbDaire);
        this.Controls.Add(lblTip);
        this.Controls.Add(cmbTip);
        this.Controls.Add(lblGiris);
        this.Controls.Add(dtpGirisTarihi);
        this.Controls.Add(btnKaydet);
        this.Controls.Add(btnIptal);

        this.AcceptButton = btnKaydet;
        this.CancelButton = btnIptal;
    }

    private void LoadDaireler()
    {
        try
        {
            using var db = new AppDbContext();
            _daireler = db.Daireler.Include(d => d.Blok).OrderBy(d => d.Blok.Ad).ThenBy(d => d.DaireNo).ToList();
            cmbDaire.Items.Clear();
            foreach (var daire in _daireler)
                cmbDaire.Items.Add($"{daire.Blok.Ad} - {daire.DaireNo}");

            if (cmbDaire.Items.Count > 0)
                cmbDaire.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Daireler yuklenirken hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnKaydet_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtAd.Text))
        {
            MessageBox.Show("Lutfen ad giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtSoyad.Text))
        {
            MessageBox.Show("Lutfen soyad giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cmbDaire.SelectedIndex < 0)
        {
            MessageBox.Show("Lutfen bir daire seciniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}

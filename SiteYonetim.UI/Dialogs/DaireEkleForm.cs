using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Dialogs;

public class DaireEkleForm : Form
{
    private ComboBox cmbBlok = null!;
    private TextBox txtDaireNo = null!;
    private NumericUpDown nudKat = null!;
    private NumericUpDown nudArsakPayi = null!;
    private ComboBox cmbTip = null!;
    private Button btnKaydet = null!;
    private Button btnIptal = null!;

    private readonly Daire? _mevcutDaire;
    private List<Blok> _bloklar = new();

    public int SeciliBlokId => (_bloklar.Count > 0 && cmbBlok.SelectedIndex >= 0)
        ? _bloklar[cmbBlok.SelectedIndex].Id : 0;
    public string DaireNo => txtDaireNo.Text.Trim();
    public int Kat => (int)nudKat.Value;
    public float ArsakPayi => (float)nudArsakPayi.Value;
    public DaireTipi Tip => (DaireTipi)cmbTip.SelectedItem!;

    public DaireEkleForm(Daire? mevcutDaire = null)
    {
        _mevcutDaire = mevcutDaire;
        InitializeComponent();
        LoadBloklar();

        if (_mevcutDaire != null)
        {
            this.Text = "Daire Duzenle";
            var blokIndex = _bloklar.FindIndex(b => b.Id == _mevcutDaire.BlokId);
            if (blokIndex >= 0) cmbBlok.SelectedIndex = blokIndex;
            txtDaireNo.Text = _mevcutDaire.DaireNo;
            nudKat.Value = _mevcutDaire.Kat;
            nudArsakPayi.Value = (decimal)_mevcutDaire.ArsakPayi;
            cmbTip.SelectedItem = _mevcutDaire.Tip;
        }
    }

    private void InitializeComponent()
    {
        this.Text = "Yeni Daire Ekle";
        this.Size = new Size(440, 400);
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
            Text = _mevcutDaire != null ? "Daire Duzenle" : "Yeni Daire Ekle",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblBaslik);

        int y = 65;
        int labelX = 20;
        int inputX = 150;
        int inputWidth = 250;

        // Blok
        var lblBlok = new Label
        {
            Text = "Blok:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        cmbBlok = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        y += 40;

        // DaireNo
        var lblDaireNo = new Label
        {
            Text = "Daire No:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        txtDaireNo = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28)
        };
        y += 40;

        // Kat
        var lblKat = new Label
        {
            Text = "Kat:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        nudKat = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            Minimum = -5,
            Maximum = 100,
            Value = 1
        };
        y += 40;

        // Arsapayı
        var lblArsaPayi = new Label
        {
            Text = "Arsa Payi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        nudArsakPayi = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            Minimum = 0,
            Maximum = 10000,
            Value = 100,
            DecimalPlaces = 2
        };
        y += 40;

        // Tip
        var lblTip = new Label
        {
            Text = "Tip:",
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
        cmbTip.Items.AddRange(Enum.GetValues<DaireTipi>().Cast<object>().ToArray());
        cmbTip.SelectedIndex = 0;
        y += 50;

        // Buttons
        btnKaydet = new Button
        {
            Text = "Kaydet",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 36),
            Location = new Point(150, y),
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
            Location = new Point(260, y),
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
        this.Controls.Add(lblBlok);
        this.Controls.Add(cmbBlok);
        this.Controls.Add(lblDaireNo);
        this.Controls.Add(txtDaireNo);
        this.Controls.Add(lblKat);
        this.Controls.Add(nudKat);
        this.Controls.Add(lblArsaPayi);
        this.Controls.Add(nudArsakPayi);
        this.Controls.Add(lblTip);
        this.Controls.Add(cmbTip);
        this.Controls.Add(btnKaydet);
        this.Controls.Add(btnIptal);

        this.AcceptButton = btnKaydet;
        this.CancelButton = btnIptal;
    }

    private void LoadBloklar()
    {
        try
        {
            using var db = new AppDbContext();
            _bloklar = db.Bloklar.OrderBy(b => b.Ad).ToList();
            cmbBlok.Items.Clear();
            foreach (var blok in _bloklar)
                cmbBlok.Items.Add(blok.Ad);

            if (cmbBlok.Items.Count > 0)
                cmbBlok.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bloklar yuklenirken hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnKaydet_Click(object? sender, EventArgs e)
    {
        if (cmbBlok.SelectedIndex < 0)
        {
            MessageBox.Show("Lutfen bir blok seciniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtDaireNo.Text))
        {
            MessageBox.Show("Lutfen daire numarasini giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}

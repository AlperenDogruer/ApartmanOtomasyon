using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Dialogs;

public class TopluTahakkukDialog : Form
{
    private NumericUpDown nudYil = null!;
    private ComboBox cmbAy = null!;
    private ComboBox cmbAidatTipi = null!;
    private NumericUpDown nudBirimTutar = null!;
    private Label lblDaireSayisi = null!;
    private Button btnOlustur = null!;
    private Button btnIptal = null!;

    private List<AidatTipi> _aidatTipleri = new();

    private static readonly string[] AyIsimleri = new[]
    {
        "Ocak", "Subat", "Mart", "Nisan", "Mayis", "Haziran",
        "Temmuz", "Agustos", "Eylul", "Ekim", "Kasim", "Aralik"
    };

    public TopluTahakkukDialog()
    {
        InitializeComponent();
        LoadAidatTipleri();
        UpdateDaireSayisi();
    }

    private void InitializeComponent()
    {
        this.Text = "Toplu Tahakkuk Olustur";
        this.Size = new Size(460, 400);
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
            Text = "Toplu Tahakkuk Olustur",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblBaslik);

        int y = 65;
        int labelX = 20;
        int inputX = 160;
        int inputWidth = 250;

        // Yil
        var lblYil = new Label
        {
            Text = "Yil:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        nudYil = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            Minimum = 2020,
            Maximum = 2050,
            Value = DateTime.Now.Year
        };
        y += 42;

        // Ay
        var lblAy = new Label
        {
            Text = "Ay:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        cmbAy = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        for (int i = 0; i < 12; i++)
            cmbAy.Items.Add($"{i + 1} - {AyIsimleri[i]}");
        cmbAy.SelectedIndex = DateTime.Now.Month - 1;
        y += 42;

        // Aidat Tipi
        var lblAidatTipi = new Label
        {
            Text = "Aidat Tipi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        cmbAidatTipi = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        y += 42;

        // Birim Tutar
        var lblTutar = new Label
        {
            Text = "Birim Tutar (₺):",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(labelX, y + 3),
            AutoSize = true
        };

        nudBirimTutar = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(inputX, y),
            Size = new Size(inputWidth, 28),
            Minimum = 0,
            Maximum = 100000,
            Value = 500,
            DecimalPlaces = 2,
            ThousandsSeparator = true
        };
        y += 50;

        // Info label
        lblDaireSayisi = new Label
        {
            Text = "Kac daire icin tahakkuk olusturulacak: ...",
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            ForeColor = Color.FromArgb(46, 134, 171),
            Location = new Point(20, y),
            Size = new Size(400, 25)
        };
        y += 40;

        // Buttons
        btnOlustur = new Button
        {
            Text = "Olustur",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(110, 38),
            Location = new Point(160, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnOlustur.FlatAppearance.BorderSize = 0;
        btnOlustur.Click += BtnOlustur_Click;

        btnIptal = new Button
        {
            Text = "Iptal",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 38),
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
        this.Controls.Add(lblYil);
        this.Controls.Add(nudYil);
        this.Controls.Add(lblAy);
        this.Controls.Add(cmbAy);
        this.Controls.Add(lblAidatTipi);
        this.Controls.Add(cmbAidatTipi);
        this.Controls.Add(lblTutar);
        this.Controls.Add(nudBirimTutar);
        this.Controls.Add(lblDaireSayisi);
        this.Controls.Add(btnOlustur);
        this.Controls.Add(btnIptal);

        this.CancelButton = btnIptal;
    }

    private void LoadAidatTipleri()
    {
        try
        {
            using var db = new AppDbContext();
            _aidatTipleri = db.AidatTipleri.Where(a => a.Aktif).OrderBy(a => a.Ad).ToList();
            cmbAidatTipi.Items.Clear();
            foreach (var tip in _aidatTipleri)
                cmbAidatTipi.Items.Add(tip.Ad);

            if (cmbAidatTipi.Items.Count > 0)
                cmbAidatTipi.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Aidat tipleri yuklenirken hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateDaireSayisi()
    {
        try
        {
            using var db = new AppDbContext();
            int daireSayisi = db.Daireler.Count();
            lblDaireSayisi.Text = $"Kac daire icin tahakkuk olusturulacak: {daireSayisi}";
        }
        catch
        {
            lblDaireSayisi.Text = "Kac daire icin tahakkuk olusturulacak: ?";
        }
    }

    private void BtnOlustur_Click(object? sender, EventArgs e)
    {
        if (cmbAidatTipi.SelectedIndex < 0)
        {
            MessageBox.Show("Lutfen aidat tipini seciniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var service = new AidatService();
            int yil = (int)nudYil.Value;
            int ay = cmbAy.SelectedIndex + 1;
            int aidatTipiId = _aidatTipleri[cmbAidatTipi.SelectedIndex].Id;
            decimal birimTutar = nudBirimTutar.Value;

            var (olusturulan, mesaj) = service.TopluTahakkukOlustur(yil, ay, aidatTipiId, birimTutar);

            MessageBox.Show(mesaj,
                olusturulan > 0 ? "Basarili" : "Bilgi",
                MessageBoxButtons.OK,
                olusturulan > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (olusturulan > 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tahakkuk olusturulurken hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

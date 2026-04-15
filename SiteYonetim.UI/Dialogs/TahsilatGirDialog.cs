using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Dialogs;

public class TahsilatGirDialog : Form
{
    private ComboBox cmbDaire = null!;
    private DataGridView dgvTahakkuklar = null!;
    private NumericUpDown nudTutar = null!;
    private ComboBox cmbOdemeTipi = null!;
    private TextBox txtAciklama = null!;
    private Button btnKaydet = null!;
    private Button btnIptal = null!;

    private List<Daire> _daireler = new();
    private List<AidatTahakkuk> _acikTahakkuklar = new();

    public TahsilatGirDialog()
    {
        InitializeComponent();
        LoadDaireler();
    }

    private void InitializeComponent()
    {
        this.Text = "Tahsilat Girisi";
        this.Size = new Size(620, 560);
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
            Text = "Tahsilat Girisi",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblBaslik);

        int y = 60;

        // Daire
        var lblDaire = new Label
        {
            Text = "Daire:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(15, y + 3),
            AutoSize = true
        };

        cmbDaire = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(130, y),
            Size = new Size(450, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbDaire.SelectedIndexChanged += CmbDaire_SelectedIndexChanged;
        y += 38;

        // DataGridView for open tahakkuks
        var lblTahakkuklar = new Label
        {
            Text = "Acik Tahakkuklar:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(15, y),
            AutoSize = true
        };
        y += 25;

        dgvTahakkuklar = new DataGridView
        {
            Location = new Point(15, y),
            Size = new Size(570, 180),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9)
        };
        dgvTahakkuklar.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
        dgvTahakkuklar.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvTahakkuklar.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvTahakkuklar.EnableHeadersVisualStyles = false;

        dgvTahakkuklar.Columns.Add("Donem", "Donem");
        dgvTahakkuklar.Columns.Add("AidatTipi", "Aidat Tipi");
        dgvTahakkuklar.Columns.Add("Tutar", "Tutar");
        dgvTahakkuklar.Columns.Add("Odenen", "Odenen");
        dgvTahakkuklar.Columns.Add("Kalan", "Kalan");

        dgvTahakkuklar.Columns["Tutar"].DefaultCellStyle.Format = "₺#,##0.00";
        dgvTahakkuklar.Columns["Odenen"].DefaultCellStyle.Format = "₺#,##0.00";
        dgvTahakkuklar.Columns["Kalan"].DefaultCellStyle.Format = "₺#,##0.00";

        dgvTahakkuklar.SelectionChanged += DgvTahakkuklar_SelectionChanged;

        y += 190;

        // Tutar
        var lblTutar = new Label
        {
            Text = "Odeme Tutari (₺):",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(15, y + 3),
            AutoSize = true
        };

        nudTutar = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(160, y),
            Size = new Size(180, 28),
            Minimum = 0,
            Maximum = 1000000,
            Value = 0,
            DecimalPlaces = 2,
            ThousandsSeparator = true
        };

        // OdemeTipi
        var lblOdemeTipi = new Label
        {
            Text = "Odeme Tipi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(360, y + 3),
            AutoSize = true
        };

        cmbOdemeTipi = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(450, y),
            Size = new Size(135, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbOdemeTipi.Items.Add("Nakit");
        cmbOdemeTipi.Items.Add("Havale/EFT");
        cmbOdemeTipi.Items.Add("Kredi Karti");
        cmbOdemeTipi.SelectedIndex = 0;
        y += 40;

        // Aciklama
        var lblAciklama = new Label
        {
            Text = "Aciklama:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(15, y + 3),
            AutoSize = true
        };

        txtAciklama = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(160, y),
            Size = new Size(425, 28)
        };
        y += 45;

        // Buttons
        btnKaydet = new Button
        {
            Text = "Kaydet",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(110, 38),
            Location = new Point(350, y),
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
            Size = new Size(100, 38),
            Location = new Point(470, y),
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
        this.Controls.Add(lblDaire);
        this.Controls.Add(cmbDaire);
        this.Controls.Add(lblTahakkuklar);
        this.Controls.Add(dgvTahakkuklar);
        this.Controls.Add(lblTutar);
        this.Controls.Add(nudTutar);
        this.Controls.Add(lblOdemeTipi);
        this.Controls.Add(cmbOdemeTipi);
        this.Controls.Add(lblAciklama);
        this.Controls.Add(txtAciklama);
        this.Controls.Add(btnKaydet);
        this.Controls.Add(btnIptal);

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

    private void CmbDaire_SelectedIndexChanged(object? sender, EventArgs e)
    {
        LoadAcikTahakkuklar();
    }

    private void LoadAcikTahakkuklar()
    {
        dgvTahakkuklar.Rows.Clear();
        _acikTahakkuklar.Clear();

        if (cmbDaire.SelectedIndex < 0) return;

        try
        {
            int daireId = _daireler[cmbDaire.SelectedIndex].Id;
            using var db = new AppDbContext();
            _acikTahakkuklar = db.AidatTahakkuklar
                .Include(t => t.AidatTipi)
                .Where(t => t.DaireId == daireId && t.Durum != TahakkukDurumu.Odenmis)
                .OrderBy(t => t.Yil).ThenBy(t => t.Ay)
                .ToList();

            foreach (var t in _acikTahakkuklar)
            {
                decimal kalan = t.Tutar - t.OdenenTutar + t.GecikmeTazminati;
                dgvTahakkuklar.Rows.Add(
                    $"{t.Yil}/{t.Ay:D2}",
                    t.AidatTipi.Ad,
                    t.Tutar,
                    t.OdenenTutar,
                    kalan
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tahakkuklar yuklenirken hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvTahakkuklar_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvTahakkuklar.SelectedRows.Count > 0 && dgvTahakkuklar.SelectedRows[0].Index < _acikTahakkuklar.Count)
        {
            var tahakkuk = _acikTahakkuklar[dgvTahakkuklar.SelectedRows[0].Index];
            decimal kalan = tahakkuk.Tutar - tahakkuk.OdenenTutar + tahakkuk.GecikmeTazminati;
            nudTutar.Value = Math.Max(0, Math.Min(kalan, nudTutar.Maximum));
        }
    }

    private OdemeTipi GetOdemeTipi()
    {
        return cmbOdemeTipi.SelectedIndex switch
        {
            1 => OdemeTipi.HavaleEFT,
            2 => OdemeTipi.KrediKarti,
            _ => OdemeTipi.Nakit
        };
    }

    private void BtnKaydet_Click(object? sender, EventArgs e)
    {
        if (dgvTahakkuklar.SelectedRows.Count == 0)
        {
            MessageBox.Show("Lutfen bir tahakkuk satiri seciniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (nudTutar.Value <= 0)
        {
            MessageBox.Show("Lutfen gecerli bir odeme tutari giriniz.", "Uyari",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int rowIndex = dgvTahakkuklar.SelectedRows[0].Index;
        if (rowIndex >= _acikTahakkuklar.Count) return;

        try
        {
            var tahakkuk = _acikTahakkuklar[rowIndex];
            var service = new AidatService();
            string sonuc = service.TahsilatKaydet(
                tahakkuk.Id,
                nudTutar.Value,
                GetOdemeTipi(),
                string.IsNullOrWhiteSpace(txtAciklama.Text) ? null : txtAciklama.Text.Trim()
            );

            MessageBox.Show(sonuc, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tahsilat kaydedilirken hata olustu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

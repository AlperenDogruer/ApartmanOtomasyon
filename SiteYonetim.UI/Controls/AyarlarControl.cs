using Microsoft.EntityFrameworkCore;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Controls;

public class AyarlarControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    // Site Bilgileri
    private TextBox txtSiteAdi = null!;
    private TextBox txtAdres = null!;
    private TextBox txtYoneticiAdi = null!;
    private TextBox txtYoneticiTelefon = null!;

    // Aidat Tipleri
    private DataGridView dgvAidatTipleri = null!;

    // Gecikme Tazminati
    private NumericUpDown nudFaizOrani = null!;
    private NumericUpDown nudSonOdemeGunu = null!;

    // Yedekleme
    private Label lblSonYedek = null!;

    public AyarlarControl()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.AutoScroll = true;
        this.Padding = new Padding(10);

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(5)
        };

        layout.Controls.Add(CreateSiteBilgileriGroup());
        layout.Controls.Add(CreateAidatTipleriGroup());
        layout.Controls.Add(CreateGecikmeGroup());
        layout.Controls.Add(CreateYedeklemeGroup());

        this.Controls.Add(layout);
    }

    #region Site Bilgileri

    private GroupBox CreateSiteBilgileriGroup()
    {
        var gb = CreateGroupBox("Site Bilgileri", 220);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        txtSiteAdi = AddTextRow(layout, 0, "Site Ad\u0131:");
        txtAdres = AddTextRow(layout, 1, "Adres:");
        txtYoneticiAdi = AddTextRow(layout, 2, "Y\u00f6netici Ad\u0131:");
        txtYoneticiTelefon = AddTextRow(layout, 3, "Y\u00f6netici Telefon:");

        var btnKaydet = new Button
        {
            Text = "Kaydet",
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 30),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 5, 0, 0)
        };
        btnKaydet.FlatAppearance.BorderSize = 0;
        btnKaydet.Click += BtnSiteBilgileriKaydet_Click;
        layout.Controls.Add(btnKaydet, 1, 4);

        gb.Controls.Add(layout);
        return gb;
    }

    private TextBox AddTextRow(TableLayoutPanel panel, int row, string label)
    {
        panel.Controls.Add(new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            AutoSize = true,
            Padding = new Padding(0, 7, 0, 0)
        }, 0, row);

        var txt = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 3, 0, 3) };
        panel.Controls.Add(txt, 1, row);
        return txt;
    }

    private void BtnSiteBilgileriKaydet_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveAyar("SiteAdi", txtSiteAdi.Text.Trim());
            SaveAyar("Adres", txtAdres.Text.Trim());
            SaveAyar("YoneticiAdi", txtYoneticiAdi.Text.Trim());
            SaveAyar("YoneticiTelefon", txtYoneticiTelefon.Text.Trim());

            MessageBox.Show("Site bilgileri kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Site bilgileri kaydedilirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Aidat Tipleri

    private GroupBox CreateAidatTipleriGroup()
    {
        var gb = CreateGroupBox("Aidat Tipleri", 280);

        dgvAidatTipleri = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(230, 230, 230),
            Font = new Font("Segoe UI", 9),
            EnableHeadersVisualStyles = false
        };
        dgvAidatTipleri.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
        dgvAidatTipleri.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvAidatTipleri.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvAidatTipleri.ColumnHeadersHeight = 32;
        dgvAidatTipleri.RowTemplate.Height = 26;
        dgvAidatTipleri.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

        dgvAidatTipleri.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Ad", Name = "Ad", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "A\u00e7\u0131klama", Name = "Aciklama", FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Aktif", Name = "Aktif", FillWeight = 15 }
        });

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnEkle = CreateSmallButton("Ekle", Color.FromArgb(46, 134, 171));
        btnEkle.Click += BtnAidatTipiEkle_Click;
        var btnDuzenle = CreateSmallButton("D\u00fczenle", Color.FromArgb(243, 156, 18));
        btnDuzenle.Click += BtnAidatTipiDuzenle_Click;
        var btnAktifPasif = CreateSmallButton("Aktif/Pasif Yap", Color.FromArgb(100, 100, 100));
        btnAktifPasif.Click += BtnAidatTipiAktifPasif_Click;
        pnlButtons.Controls.AddRange(new Control[] { btnEkle, btnDuzenle, btnAktifPasif });

        gb.Controls.Add(dgvAidatTipleri);
        gb.Controls.Add(pnlButtons);
        return gb;
    }

    private void LoadAidatTipleri()
    {
        try
        {
            dgvAidatTipleri.Rows.Clear();
            using var db = new AppDbContext();
            var list = db.AidatTipleri.OrderBy(a => a.Ad).ToList();
            foreach (var a in list)
            {
                int idx = dgvAidatTipleri.Rows.Add(a.Id, a.Ad, a.Aciklama ?? "-", a.Aktif ? "Evet" : "Hay\u0131r");
                if (!a.Aktif)
                    dgvAidatTipleri.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Aidat tipleri y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnAidatTipiEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new AidatTipiDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                db.AidatTipleri.Add(new AidatTipi { Ad = dlg.TipAdi, Aciklama = dlg.TipAciklama, Aktif = true });
                db.SaveChanges();
                LoadAidatTipleri();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Aidat tipi eklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnAidatTipiDuzenle_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvAidatTipleri.CurrentRow == null) return;
            int id = (int)dgvAidatTipleri.CurrentRow.Cells["Id"].Value;

            using var db = new AppDbContext();
            var tip = db.AidatTipleri.Find(id);
            if (tip == null) return;

            using var dlg = new AidatTipiDialog(tip.Ad, tip.Aciklama);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tip.Ad = dlg.TipAdi;
                tip.Aciklama = dlg.TipAciklama;
                db.SaveChanges();
                LoadAidatTipleri();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Aidat tipi d\u00fczenlenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnAidatTipiAktifPasif_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvAidatTipleri.CurrentRow == null) return;
            int id = (int)dgvAidatTipleri.CurrentRow.Cells["Id"].Value;

            using var db = new AppDbContext();
            var tip = db.AidatTipleri.Find(id);
            if (tip == null) return;

            tip.Aktif = !tip.Aktif;
            db.SaveChanges();
            LoadAidatTipleri();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Durum de\u011fi\u015ftirilirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Gecikme Tazminati

    private GroupBox CreateGecikmeGroup()
    {
        var gb = CreateGroupBox("Gecikme Tazminat\u0131", 130);

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };

        nudFaizOrani = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 2, Value = 2, Width = 80, Font = new Font("Segoe UI", 10) };
        nudSonOdemeGunu = new NumericUpDown { Minimum = 0, Maximum = 31, Value = 0, Width = 60, Font = new Font("Segoe UI", 10) };

        var btnKaydet = new Button
        {
            Text = "Kaydet",
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 30),
            Cursor = Cursors.Hand,
            Margin = new Padding(15, 0, 0, 0)
        };
        btnKaydet.FlatAppearance.BorderSize = 0;
        btnKaydet.Click += BtnGecikmeKaydet_Click;

        layout.Controls.AddRange(new Control[]
        {
            new Label { Text = "Ayl\u0131k faiz oran\u0131 (%):", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            nudFaizOrani,
            new Label { Text = "Son \u00f6deme g\u00fcn\u00fc (0=ay sonu):", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(15, 5, 0, 0) },
            nudSonOdemeGunu,
            btnKaydet
        });

        gb.Controls.Add(layout);
        return gb;
    }

    private void BtnGecikmeKaydet_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveAyar("GecikmeFaizOrani", nudFaizOrani.Value.ToString());
            SaveAyar("SonOdemeGunu", nudSonOdemeGunu.Value.ToString());
            MessageBox.Show("Gecikme tazminat\u0131 ayarlar\u0131 kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ayarlar kaydedilirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Yedekleme

    private GroupBox CreateYedeklemeGroup()
    {
        var gb = CreateGroupBox("Yedekleme", 100);

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };

        var btnYedekle = new Button
        {
            Text = "Veritaban\u0131n\u0131 Yedekle",
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(180, 30),
            Cursor = Cursors.Hand
        };
        btnYedekle.FlatAppearance.BorderSize = 0;
        btnYedekle.Click += BtnYedekle_Click;

        lblSonYedek = new Label
        {
            Text = "Son yedek: -",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            AutoSize = true,
            Padding = new Padding(15, 7, 0, 0)
        };

        layout.Controls.AddRange(new Control[] { btnYedekle, lblSonYedek });
        gb.Controls.Add(layout);
        return gb;
    }

    private void BtnYedekle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var fbd = new FolderBrowserDialog { Description = "Yedek dosyas\u0131n\u0131n kaydedilece\u011fi klas\u00f6r\u00fc se\u00e7in" };
            if (fbd.ShowDialog() != DialogResult.OK) return;

            string dbFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SiteYonetim");
            string dbPath = Path.Combine(dbFolder, "siteYonetim.db");

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Veritaban\u0131 dosyas\u0131 bulunamad\u0131.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string backupName = $"siteYonetim_backup_{DateTime.Now:yyyyMMdd_HHmm}.db";
            string backupPath = Path.Combine(fbd.SelectedPath, backupName);
            File.Copy(dbPath, backupPath, overwrite: true);

            string yedekTarihi = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            SaveAyar("SonYedekTarihi", yedekTarihi);
            lblSonYedek.Text = $"Son yedek: {yedekTarihi}";

            MessageBox.Show($"Veritaban\u0131 ba\u015far\u0131yla yedeklendi:\n{backupPath}", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Yedekleme s\u0131ras\u0131nda hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Helpers

    private void LoadSettings()
    {
        try
        {
            using var db = new AppDbContext();

            txtSiteAdi.Text = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SiteAdi")?.Deger ?? "";
            txtAdres.Text = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "Adres")?.Deger ?? "";
            txtYoneticiAdi.Text = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "YoneticiAdi")?.Deger ?? "";
            txtYoneticiTelefon.Text = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "YoneticiTelefon")?.Deger ?? "";

            var faiz = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "GecikmeFaizOrani");
            if (faiz != null && decimal.TryParse(faiz.Deger, out var fOran))
                nudFaizOrani.Value = fOran;

            var sonOdeme = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SonOdemeGunu");
            if (sonOdeme != null && int.TryParse(sonOdeme.Deger, out var gun))
                nudSonOdemeGunu.Value = gun;

            var sonYedek = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SonYedekTarihi");
            lblSonYedek.Text = sonYedek != null ? $"Son yedek: {sonYedek.Deger}" : "Son yedek: -";

            LoadAidatTipleri();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ayarlar y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveAyar(string anahtar, string deger)
    {
        using var db = new AppDbContext();
        var ayar = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == anahtar);
        if (ayar != null)
        {
            ayar.Deger = deger;
        }
        else
        {
            db.SiteAyarlar.Add(new SiteAyar { Anahtar = anahtar, Deger = deger });
        }
        db.SaveChanges();
    }

    private GroupBox CreateGroupBox(string text, int height)
    {
        return new GroupBox
        {
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Width = 850,
            Height = height,
            Margin = new Padding(5, 5, 5, 10),
            Padding = new Padding(10)
        };
    }

    private Button CreateSmallButton(string text, Color color)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Size = new Size(110, 28),
            Margin = new Padding(3),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    #endregion
}

/// <summary>
/// Simple dialog for adding/editing AidatTipi.
/// </summary>
public class AidatTipiDialog : Form
{
    public string TipAdi { get; private set; } = "";
    public string? TipAciklama { get; private set; }

    public AidatTipiDialog(string? ad = null, string? aciklama = null)
    {
        this.Text = ad == null ? "Aidat Tipi Ekle" : "Aidat Tipi D\u00fczenle";
        this.Size = new Size(380, 200);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(15)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "Ad:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, 0, 0);
        var txtAd = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Text = ad ?? "" };
        layout.Controls.Add(txtAd, 1, 0);

        layout.Controls.Add(new Label { Text = "A\u00e7\u0131klama:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, 0, 1);
        var txtAciklama = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Text = aciklama ?? "" };
        layout.Controls.Add(txtAciklama, 1, 1);

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(10, 5, 10, 5)
        };

        var btnIptal = new Button { Text = "\u0130ptal", DialogResult = DialogResult.Cancel, Size = new Size(80, 30), FlatStyle = FlatStyle.Flat };
        var btnKaydet = new Button
        {
            Text = "Kaydet",
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 30),
            Cursor = Cursors.Hand
        };
        btnKaydet.FlatAppearance.BorderSize = 0;
        btnKaydet.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtAd.Text))
            {
                MessageBox.Show("Ad bo\u015f olamaz.", "Uyar\u0131", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TipAdi = txtAd.Text.Trim();
            TipAciklama = string.IsNullOrWhiteSpace(txtAciklama.Text) ? null : txtAciklama.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        };

        pnlButtons.Controls.AddRange(new Control[] { btnIptal, btnKaydet });
        this.Controls.Add(layout);
        this.Controls.Add(pnlButtons);
        this.AcceptButton = btnKaydet;
        this.CancelButton = btnIptal;
    }
}

using System.Data;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Enums;
using SiteYonetim.Core.Helpers;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.UI.Controls;

public class GelirGiderControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    private static readonly string[] GelirKategorileri = { "T\u00fcm\u00fc", "Aidat", "Kira", "Ba\u011f\u0131\u015f", "Di\u011fer" };
    private static readonly string[] GiderKategorileri = { "T\u00fcm\u00fc", "Temizlik", "G\u00fcvenlik", "Bak\u0131m", "Elektrik", "Su", "Do\u011falgaz", "Personel", "Sigorta", "Di\u011fer" };

    // Tab 1 - Gelirler
    private DateTimePicker dtpGelirBas = null!;
    private DateTimePicker dtpGelirBit = null!;
    private ComboBox cmbGelirKategori = null!;
    private DataGridView dgvGelirler = null!;
    private Label lblGelirToplam = null!;

    // Tab 2 - Giderler
    private DateTimePicker dtpGiderBas = null!;
    private DateTimePicker dtpGiderBit = null!;
    private ComboBox cmbGiderKategori = null!;
    private DataGridView dgvGiderler = null!;
    private Label lblGiderToplam = null!;

    // Tab 3 - Kasa Ozeti
    private DateTimePicker dtpKasaBas = null!;
    private DateTimePicker dtpKasaBit = null!;
    private Label lblToplamGelir = null!;
    private Label lblToplamGider = null!;
    private Label lblNetBakiye = null!;
    private DataGridView dgvKategoriOzet = null!;

    public GelirGiderControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(240, 244, 248);

        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9)
        };

        tabControl.TabPages.Add(CreateGelirlerTab());
        tabControl.TabPages.Add(CreateGiderlerTab());
        tabControl.TabPages.Add(CreateKasaOzetiTab());

        this.Controls.Add(tabControl);

        this.Load += (s, e) => { LoadGelirler(); LoadGiderler(); };
    }

    #region Tab 1 - Gelirler

    private TabPage CreateGelirlerTab()
    {
        var tab = new TabPage("Gelirler") { BackColor = Color.White };

        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 50, Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        dtpGelirBas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) };
        dtpGelirBit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = DateTime.Now };
        cmbGelirKategori = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = new Font("Segoe UI", 10) };
        cmbGelirKategori.Items.AddRange(GelirKategorileri);
        cmbGelirKategori.SelectedIndex = 0;

        var btnFiltrele = CreateButton("Filtrele", Color.FromArgb(46, 134, 171));
        btnFiltrele.Click += (s, e) => LoadGelirler();

        pnlFilter.Controls.AddRange(new Control[]
        {
            new Label { Text = "Ba\u015f:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) }, dtpGelirBas,
            new Label { Text = "Bit:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(5, 5, 0, 0) }, dtpGelirBit,
            new Label { Text = "Kategori:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(5, 5, 0, 0) }, cmbGelirKategori,
            btnFiltrele
        });

        var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 90 };

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 45, Padding = new Padding(10, 8, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };
        var btnEkle = CreateButton("+ Gelir Ekle", Color.FromArgb(46, 134, 171));
        btnEkle.Click += BtnGelirEkle_Click;
        var btnDuzenle = CreateButton("\u270f D\u00fczenle", Color.FromArgb(243, 156, 18));
        btnDuzenle.Click += BtnGelirDuzenle_Click;
        var btnSil = CreateButton("\ud83d\uddd1 Sil", Color.FromArgb(231, 76, 60));
        btnSil.Click += BtnGelirSil_Click;
        pnlButtons.Controls.AddRange(new Control[] { btnEkle, btnDuzenle, btnSil });

        lblGelirToplam = new Label
        {
            Text = "Toplam: \u20ba0,00",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(39, 174, 96),
            Dock = DockStyle.Bottom,
            Height = 35,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 15, 0)
        };

        pnlBottom.Controls.Add(lblGelirToplam);
        pnlBottom.Controls.Add(pnlButtons);

        dgvGelirler = CreateGrid();
        dgvGelirler.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Tarih", Name = "Tarih" },
            new DataGridViewTextBoxColumn { HeaderText = "A\u00e7\u0131klama", Name = "Aciklama", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Kategori", Name = "Kategori" },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", Name = "Tutar", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6deme Tipi", Name = "OdemeTipi" },
            new DataGridViewTextBoxColumn { HeaderText = "Belge No", Name = "BelgeNo" }
        });

        tab.Controls.Add(dgvGelirler);
        tab.Controls.Add(pnlBottom);
        tab.Controls.Add(pnlFilter);
        return tab;
    }

    private void LoadGelirler()
    {
        try
        {
            dgvGelirler.Rows.Clear();
            var service = new GelirGiderService();
            string? kategori = cmbGelirKategori.SelectedItem?.ToString();
            var list = service.GelirListele(dtpGelirBas.Value.Date, dtpGelirBit.Value.Date, kategori);

            decimal toplam = 0;
            foreach (var g in list)
            {
                dgvGelirler.Rows.Add(g.Id, g.Tarih.ToString("dd.MM.yyyy"), g.Aciklama, g.Kategori, g.Tutar, g.OdemeTipi.ToString(), g.BelgeNo ?? "-");
                toplam += g.Tutar;
            }
            lblGelirToplam.Text = $"Toplam: {toplam:\u20ba#,##0.00}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gelirler y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGelirEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new GelirGiderForm("Gelir Ekle", GelirKategorileri.Skip(1).ToArray());
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var service = new GelirGiderService();
                service.GelirKaydet(new GelirKalemi
                {
                    Aciklama = dlg.Aciklama,
                    Tutar = dlg.Tutar,
                    Tarih = dlg.Tarih,
                    Kategori = dlg.Kategori,
                    OdemeTipi = dlg.OdemeTipiSecim,
                    BelgeNo = dlg.BelgeNo
                });
                LoadGelirler();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gelir eklenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGelirDuzenle_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvGelirler.CurrentRow == null) return;
            int id = (int)dgvGelirler.CurrentRow.Cells["Id"].Value;

            using var db = new AppDbContext();
            var gelir = db.GelirKalemleri.Find(id);
            if (gelir == null) return;

            using var dlg = new GelirGiderForm("Gelir D\u00fczenle", GelirKategorileri.Skip(1).ToArray(), gelir.Aciklama, gelir.Tutar, gelir.Tarih, gelir.Kategori, gelir.OdemeTipi, gelir.BelgeNo);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                gelir.Aciklama = dlg.Aciklama;
                gelir.Tutar = dlg.Tutar;
                gelir.Tarih = dlg.Tarih;
                gelir.Kategori = dlg.Kategori;
                gelir.OdemeTipi = dlg.OdemeTipiSecim;
                gelir.BelgeNo = dlg.BelgeNo;

                var service = new GelirGiderService();
                service.GelirGuncelle(gelir);
                LoadGelirler();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gelir d\u00fczenlenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGelirSil_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvGelirler.CurrentRow == null) return;
            if (MessageBox.Show("Bu gelir kayd\u0131 silinecek. Emin misiniz?", "Silme Onay\u0131",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = (int)dgvGelirler.CurrentRow.Cells["Id"].Value;
                var service = new GelirGiderService();
                service.GelirSil(id);
                LoadGelirler();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gelir silinirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Tab 2 - Giderler

    private TabPage CreateGiderlerTab()
    {
        var tab = new TabPage("Giderler") { BackColor = Color.White };

        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 50, Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        dtpGiderBas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) };
        dtpGiderBit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = DateTime.Now };
        cmbGiderKategori = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = new Font("Segoe UI", 10) };
        cmbGiderKategori.Items.AddRange(GiderKategorileri);
        cmbGiderKategori.SelectedIndex = 0;

        var btnFiltrele = CreateButton("Filtrele", Color.FromArgb(46, 134, 171));
        btnFiltrele.Click += (s, e) => LoadGiderler();

        pnlFilter.Controls.AddRange(new Control[]
        {
            new Label { Text = "Ba\u015f:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) }, dtpGiderBas,
            new Label { Text = "Bit:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(5, 5, 0, 0) }, dtpGiderBit,
            new Label { Text = "Kategori:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(5, 5, 0, 0) }, cmbGiderKategori,
            btnFiltrele
        });

        var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 90 };

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 45, Padding = new Padding(10, 8, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };
        var btnEkle = CreateButton("+ Gider Ekle", Color.FromArgb(46, 134, 171));
        btnEkle.Click += BtnGiderEkle_Click;
        var btnDuzenle = CreateButton("\u270f D\u00fczenle", Color.FromArgb(243, 156, 18));
        btnDuzenle.Click += BtnGiderDuzenle_Click;
        var btnSil = CreateButton("\ud83d\uddd1 Sil", Color.FromArgb(231, 76, 60));
        btnSil.Click += BtnGiderSil_Click;
        pnlButtons.Controls.AddRange(new Control[] { btnEkle, btnDuzenle, btnSil });

        lblGiderToplam = new Label
        {
            Text = "Toplam: \u20ba0,00",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60),
            Dock = DockStyle.Bottom,
            Height = 35,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 15, 0)
        };

        pnlBottom.Controls.Add(lblGiderToplam);
        pnlBottom.Controls.Add(pnlButtons);

        dgvGiderler = CreateGrid();
        dgvGiderler.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Tarih", Name = "Tarih" },
            new DataGridViewTextBoxColumn { HeaderText = "A\u00e7\u0131klama", Name = "Aciklama", FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Kategori", Name = "Kategori" },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", Name = "Tutar", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6deme Tipi", Name = "OdemeTipi" },
            new DataGridViewTextBoxColumn { HeaderText = "Belge No", Name = "BelgeNo" },
            new DataGridViewTextBoxColumn { HeaderText = "Tedarik\u00e7i", Name = "Tedarikci" }
        });

        tab.Controls.Add(dgvGiderler);
        tab.Controls.Add(pnlBottom);
        tab.Controls.Add(pnlFilter);
        return tab;
    }

    private void LoadGiderler()
    {
        try
        {
            dgvGiderler.Rows.Clear();
            var service = new GelirGiderService();
            string? kategori = cmbGiderKategori.SelectedItem?.ToString();
            var list = service.GiderListele(dtpGiderBas.Value.Date, dtpGiderBit.Value.Date, kategori);

            decimal toplam = 0;
            foreach (var g in list)
            {
                dgvGiderler.Rows.Add(g.Id, g.Tarih.ToString("dd.MM.yyyy"), g.Aciklama, g.Kategori, g.Tutar, g.OdemeTipi.ToString(), g.BelgeNo ?? "-", g.Tedarikci ?? "-");
                toplam += g.Tutar;
            }
            lblGiderToplam.Text = $"Toplam: {toplam:\u20ba#,##0.00}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Giderler y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGiderEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new GelirGiderForm("Gider Ekle", GiderKategorileri.Skip(1).ToArray(), tedarikciGoster: true);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var service = new GelirGiderService();
                service.GiderKaydet(new GiderKalemi
                {
                    Aciklama = dlg.Aciklama,
                    Tutar = dlg.Tutar,
                    Tarih = dlg.Tarih,
                    Kategori = dlg.Kategori,
                    OdemeTipi = dlg.OdemeTipiSecim,
                    BelgeNo = dlg.BelgeNo,
                    Tedarikci = dlg.Tedarikci
                });
                LoadGiderler();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gider eklenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGiderDuzenle_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvGiderler.CurrentRow == null) return;
            int id = (int)dgvGiderler.CurrentRow.Cells["Id"].Value;

            using var db = new AppDbContext();
            var gider = db.GiderKalemleri.Find(id);
            if (gider == null) return;

            using var dlg = new GelirGiderForm("Gider D\u00fczenle", GiderKategorileri.Skip(1).ToArray(),
                gider.Aciklama, gider.Tutar, gider.Tarih, gider.Kategori, gider.OdemeTipi, gider.BelgeNo, true, gider.Tedarikci);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                gider.Aciklama = dlg.Aciklama;
                gider.Tutar = dlg.Tutar;
                gider.Tarih = dlg.Tarih;
                gider.Kategori = dlg.Kategori;
                gider.OdemeTipi = dlg.OdemeTipiSecim;
                gider.BelgeNo = dlg.BelgeNo;
                gider.Tedarikci = dlg.Tedarikci;

                var service = new GelirGiderService();
                service.GiderGuncelle(gider);
                LoadGiderler();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gider d\u00fczenlenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGiderSil_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvGiderler.CurrentRow == null) return;
            if (MessageBox.Show("Bu gider kayd\u0131 silinecek. Emin misiniz?", "Silme Onay\u0131",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = (int)dgvGiderler.CurrentRow.Cells["Id"].Value;
                var service = new GelirGiderService();
                service.GiderSil(id);
                LoadGiderler();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gider silinirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Tab 3 - Kasa Ozeti

    private TabPage CreateKasaOzetiTab()
    {
        var tab = new TabPage("Kasa \u00d6zeti") { BackColor = Color.White };

        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 50, Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        dtpKasaBas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = new DateTime(DateTime.Now.Year, 1, 1) };
        dtpKasaBit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = DateTime.Now };

        var btnHesapla = CreateButton("Hesapla", Color.FromArgb(46, 134, 171));
        btnHesapla.Click += (s, e) => LoadKasaOzeti();

        pnlFilter.Controls.AddRange(new Control[]
        {
            new Label { Text = "Ba\u015flang\u0131\u00e7:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) }, dtpKasaBas,
            new Label { Text = "Biti\u015f:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) }, dtpKasaBit,
            btnHesapla
        });

        // Summary cards
        var pnlSummary = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 100, Padding = new Padding(10),
            FlowDirection = FlowDirection.LeftToRight
        };

        lblToplamGelir = CreateSummaryLabel("Toplam Gelir", "\u20ba0,00", Color.FromArgb(39, 174, 96));
        lblToplamGider = CreateSummaryLabel("Toplam Gider", "\u20ba0,00", Color.FromArgb(231, 76, 60));
        lblNetBakiye = CreateSummaryLabel("Net Bakiye", "\u20ba0,00", Color.FromArgb(30, 58, 95));

        pnlSummary.Controls.AddRange(new Control[] { lblToplamGelir.Parent!, lblToplamGider.Parent!, lblNetBakiye.Parent! });

        // Export buttons
        var pnlExport = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnPdf = CreateButton("Rapor \u0130ndir PDF", Color.FromArgb(231, 76, 60), 140);
        btnPdf.Click += BtnKasaPdf_Click;
        var btnExcel = CreateButton("Excel \u0130ndir", Color.FromArgb(39, 174, 96));
        btnExcel.Click += BtnKasaExcel_Click;
        pnlExport.Controls.AddRange(new Control[] { btnPdf, btnExcel });

        dgvKategoriOzet = CreateGrid();
        dgvKategoriOzet.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Kategori", Name = "Kategori", FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", Name = "Tutar", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight }, FillWeight = 30 }
        });

        tab.Controls.Add(dgvKategoriOzet);
        tab.Controls.Add(pnlExport);
        tab.Controls.Add(pnlSummary);
        tab.Controls.Add(pnlFilter);
        return tab;
    }

    private Label CreateSummaryLabel(string title, string value, Color color)
    {
        var panel = new Panel
        {
            Width = 220, Height = 80, Margin = new Padding(8),
            BackColor = Color.White, Padding = new Padding(10)
        };
        var accent = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = color };
        var lblTitle = new Label
        {
            Text = title, Dock = DockStyle.Top, Height = 22,
            Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleLeft
        };
        var lblVal = new Label
        {
            Text = value, Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = color, TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(lblVal);
        panel.Controls.Add(lblTitle);
        panel.Controls.Add(accent);
        return lblVal;
    }

    private void LoadKasaOzeti()
    {
        try
        {
            var service = new GelirGiderService();
            var (gelir, gider, net) = service.KasaDurumu(dtpKasaBas.Value.Date, dtpKasaBit.Value.Date);

            lblToplamGelir.Text = gelir.ToString("\u20ba#,##0.00");
            lblToplamGider.Text = gider.ToString("\u20ba#,##0.00");
            lblNetBakiye.Text = net.ToString("\u20ba#,##0.00");
            lblNetBakiye.ForeColor = net >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(231, 76, 60);

            // Category breakdown
            dgvKategoriOzet.Rows.Clear();
            using var db = new AppDbContext();
            var giderGruplar = db.GiderKalemleri
                .Where(g => g.Tarih >= dtpKasaBas.Value.Date && g.Tarih <= dtpKasaBit.Value.Date)
                .ToList()
                .GroupBy(g => g.Kategori)
                .Select(g => new { Kategori = g.Key, Tutar = g.Sum(x => x.Tutar) })
                .OrderByDescending(g => g.Tutar)
                .ToList();

            foreach (var g in giderGruplar)
                dgvKategoriOzet.Rows.Add(g.Kategori, g.Tutar);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kasa \u00f6zeti y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private DataTable GetKasaRaporDataTable()
    {
        var service = new RaporService();
        return service.GelirGiderRaporu(dtpKasaBas.Value.Date, dtpKasaBit.Value.Date);
    }

    private void BtnKasaPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            using var sfd = new SaveFileDialog { Filter = "PDF Dosyas\u0131|*.pdf", FileName = "KasaOzeti.pdf" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var dt = GetKasaRaporDataTable();
                string[] basliklar = { "T\u00fcr", "Tarih", "A\u00e7\u0131klama", "Kategori", "Tutar" };
                ExportHelper.ExportToPdf(dt, basliklar, "Gelir-Gider Raporu", sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnKasaExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            using var sfd = new SaveFileDialog { Filter = "Excel Dosyas\u0131|*.xlsx", FileName = "KasaOzeti.xlsx" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var dt = GetKasaRaporDataTable();
                string[] basliklar = { "T\u00fcr", "Tarih", "A\u00e7\u0131klama", "Kategori", "Tutar" };
                ExportHelper.ExportToExcel(dt, basliklar, "Gelir-Gider Raporu", sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Helpers

    private DataGridView CreateGrid()
    {
        var dgv = new DataGridView
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
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 35;
        dgv.RowTemplate.Height = 28;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        return dgv;
    }

    private Button CreateButton(string text, Color color, int width = 130)
    {
        var btn = new Button
        {
            Text = text, FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), Size = new Size(width, 30),
            Margin = new Padding(3), Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    #endregion
}

/// <summary>
/// Inline form for adding/editing Gelir or Gider entries.
/// </summary>
public class GelirGiderForm : Form
{
    public string Aciklama { get; private set; } = "";
    public decimal Tutar { get; private set; }
    public DateTime Tarih { get; private set; }
    public string Kategori { get; private set; } = "";
    public OdemeTipi OdemeTipiSecim { get; private set; }
    public string? BelgeNo { get; private set; }
    public string? Tedarikci { get; private set; }

    private TextBox txtAciklama = null!;
    private NumericUpDown nudTutar = null!;
    private DateTimePicker dtpTarih = null!;
    private ComboBox cmbKategori = null!;
    private ComboBox cmbOdemeTipi = null!;
    private TextBox txtBelgeNo = null!;
    private TextBox txtTedarikci = null!;

    public GelirGiderForm(string baslik, string[] kategoriler,
        string? aciklama = null, decimal? tutar = null, DateTime? tarih = null,
        string? kategori = null, OdemeTipi? odemeTipi = null, string? belgeNo = null,
        bool tedarikciGoster = false, string? tedarikci = null)
    {
        this.Text = baslik;
        this.Size = new Size(420, tedarikciGoster ? 420 : 380);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(15),
            RowCount = tedarikciGoster ? 8 : 7
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int row = 0;

        AddLabel(layout, "A\u00e7\u0131klama:", row);
        txtAciklama = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Text = aciklama ?? "" };
        layout.Controls.Add(txtAciklama, 1, row++);

        AddLabel(layout, "Tutar:", row);
        nudTutar = new NumericUpDown { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Maximum = 99999999, DecimalPlaces = 2, Value = tutar ?? 0 };
        layout.Controls.Add(nudTutar, 1, row++);

        AddLabel(layout, "Tarih:", row);
        dtpTarih = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Value = tarih ?? DateTime.Now };
        layout.Controls.Add(dtpTarih, 1, row++);

        AddLabel(layout, "Kategori:", row);
        cmbKategori = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbKategori.Items.AddRange(kategoriler);
        if (kategori != null) cmbKategori.SelectedItem = kategori;
        else if (cmbKategori.Items.Count > 0) cmbKategori.SelectedIndex = 0;
        layout.Controls.Add(cmbKategori, 1, row++);

        AddLabel(layout, "\u00d6deme Tipi:", row);
        cmbOdemeTipi = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbOdemeTipi.Items.AddRange(Enum.GetNames<OdemeTipi>());
        cmbOdemeTipi.SelectedItem = (odemeTipi ?? OdemeTipi.Nakit).ToString();
        layout.Controls.Add(cmbOdemeTipi, 1, row++);

        AddLabel(layout, "Belge No:", row);
        txtBelgeNo = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Text = belgeNo ?? "" };
        layout.Controls.Add(txtBelgeNo, 1, row++);

        if (tedarikciGoster)
        {
            AddLabel(layout, "Tedarik\u00e7i:", row);
            txtTedarikci = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Text = tedarikci ?? "" };
            layout.Controls.Add(txtTedarikci, 1, row++);
        }

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(10, 10, 10, 5)
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
            if (string.IsNullOrWhiteSpace(txtAciklama.Text))
            {
                MessageBox.Show("A\u00e7\u0131klama bo\u015f olamaz.", "Uyar\u0131", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Aciklama = txtAciklama.Text.Trim();
            Tutar = nudTutar.Value;
            Tarih = dtpTarih.Value.Date;
            Kategori = cmbKategori.SelectedItem?.ToString() ?? "";
            OdemeTipiSecim = Enum.Parse<OdemeTipi>(cmbOdemeTipi.SelectedItem?.ToString() ?? "Nakit");
            BelgeNo = string.IsNullOrWhiteSpace(txtBelgeNo.Text) ? null : txtBelgeNo.Text.Trim();
            Tedarikci = txtTedarikci != null && !string.IsNullOrWhiteSpace(txtTedarikci.Text) ? txtTedarikci.Text.Trim() : null;
            DialogResult = DialogResult.OK;
            Close();
        };

        pnlButtons.Controls.AddRange(new Control[] { btnIptal, btnKaydet });
        this.Controls.Add(layout);
        this.Controls.Add(pnlButtons);
        this.AcceptButton = btnKaydet;
        this.CancelButton = btnIptal;
    }

    private void AddLabel(TableLayoutPanel panel, string text, int row)
    {
        panel.Controls.Add(new Label
        {
            Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95), AutoSize = true,
            Padding = new Padding(0, 5, 0, 0)
        }, 0, row);
    }
}

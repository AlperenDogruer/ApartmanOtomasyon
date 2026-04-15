using System.Data;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Enums;
using SiteYonetim.Core.Helpers;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Dialogs;

namespace SiteYonetim.UI.Controls;

public class AidatControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    private static readonly string[] AyAdlari = { "Ocak", "\u015eubat", "Mart", "Nisan", "May\u0131s", "Haziran",
        "Temmuz", "A\u011fustos", "Eyl\u00fcl", "Ekim", "Kas\u0131m", "Aral\u0131k" };

    // Tab 1
    private NumericUpDown nudYil1 = null!;
    private ComboBox cmbAy1 = null!;
    private DataGridView dgvTahakkuk = null!;

    // Tab 2
    private ComboBox cmbDaire = null!;
    private DateTimePicker dtpEkstreBas = null!;
    private DateTimePicker dtpEkstreBit = null!;
    private DataGridView dgvEkstre = null!;

    // Tab 3
    private NumericUpDown nudYil3 = null!;
    private ComboBox cmbAy3 = null!;
    private DataGridView dgvGecikme = null!;

    public AidatControl()
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

        tabControl.TabPages.Add(CreateTab1());
        tabControl.TabPages.Add(CreateTab2());
        tabControl.TabPages.Add(CreateTab3());

        this.Controls.Add(tabControl);
    }

    #region Tab 1 - Tahakkuk Listesi

    private TabPage CreateTab1()
    {
        var tab = new TabPage("Tahakkuk Listesi") { BackColor = Color.White };

        // Filter panel
        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        nudYil1 = new NumericUpDown { Minimum = 2020, Maximum = 2050, Value = DateTime.Now.Year, Width = 80, Font = new Font("Segoe UI", 10) };
        cmbAy1 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = new Font("Segoe UI", 10) };
        cmbAy1.Items.AddRange(AyAdlari);
        cmbAy1.SelectedIndex = DateTime.Now.Month - 1;

        var btnListele = CreateButton("Listele", Color.FromArgb(46, 134, 171));
        btnListele.Click += (s, e) => LoadTahakkukListesi();

        pnlFilter.Controls.AddRange(new Control[]
        {
            new Label { Text = "Y\u0131l:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            nudYil1,
            new Label { Text = "Ay:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            cmbAy1,
            btnListele
        });

        // Buttons panel
        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnTopluTahakkuk = CreateButton("Toplu Tahakkuk Olu\u015ftur", Color.FromArgb(46, 134, 171), 180);
        btnTopluTahakkuk.Click += BtnTopluTahakkuk_Click;
        var btnTahsilatGir = CreateButton("Tahsilat Gir", Color.FromArgb(39, 174, 96));
        btnTahsilatGir.Click += BtnTahsilatGir_Click;
        var btnGecikmeTaz = CreateButton("Gecikme Tazminat\u0131 Uygula", Color.FromArgb(243, 156, 18), 190);
        btnGecikmeTaz.Click += BtnGecikmeTazminatiUygula_Click;

        pnlButtons.Controls.AddRange(new Control[] { btnTopluTahakkuk, btnTahsilatGir, btnGecikmeTaz });

        dgvTahakkuk = CreateGrid();
        dgvTahakkuk.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Blok", Name = "Blok" },
            new DataGridViewTextBoxColumn { HeaderText = "Daire No", Name = "DaireNo" },
            new DataGridViewTextBoxColumn { HeaderText = "Sakin", Name = "Sakin" },
            new DataGridViewTextBoxColumn { HeaderText = "Aidat Tipi", Name = "AidatTipi" },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", Name = "Tutar", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6denen", Name = "Odenen", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Gecikme Taz.", Name = "GecikmeTaz", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", Name = "Durum" }
        });

        tab.Controls.Add(dgvTahakkuk);
        tab.Controls.Add(pnlButtons);
        tab.Controls.Add(pnlFilter);

        // Auto load
        this.Load += (s, e) => LoadTahakkukListesi();

        return tab;
    }

    private void LoadTahakkukListesi()
    {
        try
        {
            dgvTahakkuk.Rows.Clear();
            int yil = (int)nudYil1.Value;
            int ay = cmbAy1.SelectedIndex + 1;

            using var db = new AppDbContext();
            var list = db.AidatTahakkuklar
                .Include(t => t.Daire).ThenInclude(d => d.Blok)
                .Include(t => t.Daire).ThenInclude(d => d.Sakinler)
                .Include(t => t.AidatTipi)
                .Where(t => t.Yil == yil && t.Ay == ay)
                .OrderBy(t => t.Daire.Blok.Ad).ThenBy(t => t.Daire.DaireNo)
                .ToList();

            foreach (var t in list)
            {
                var sakin = t.Daire.Sakinler.FirstOrDefault(s => s.Aktif);
                string durumStr = t.Durum switch
                {
                    TahakkukDurumu.Odenmis => "\u00d6denmi\u015f",
                    TahakkukDurumu.KismiOdeme => "K\u0131smi \u00d6deme",
                    _ => "\u00d6denmemi\u015f"
                };

                int idx = dgvTahakkuk.Rows.Add(t.Id, t.Daire.Blok.Ad, t.Daire.DaireNo,
                    sakin != null ? $"{sakin.Ad} {sakin.Soyad}" : "-",
                    t.AidatTipi.Ad, t.Tutar, t.OdenenTutar, t.GecikmeTazminati, durumStr);

                var row = dgvTahakkuk.Rows[idx];
                if (t.Durum == TahakkukDurumu.Odenmemis)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                }
                else if (t.Durum == TahakkukDurumu.KismiOdeme)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 231);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(243, 156, 18);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(234, 250, 241);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(39, 174, 96);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tahakkuk listesi y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnTopluTahakkuk_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new TopluTahakkukDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadTahakkukListesi();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Toplu tahakkuk olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnTahsilatGir_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvTahakkuk.CurrentRow == null)
            {
                MessageBox.Show("L\u00fctfen tahsilat girmek i\u00e7in bir tahakkuk se\u00e7in.", "Uyar\u0131",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new TahsilatGirDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadTahakkukListesi();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tahsilat girilirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGecikmeTazminatiUygula_Click(object? sender, EventArgs e)
    {
        try
        {
            using var db = new AppDbContext();
            var ayarFaiz = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "GecikmeFaizOrani");
            decimal oran = 2m;
            if (ayarFaiz != null && decimal.TryParse(ayarFaiz.Deger, out var parsed))
                oran = parsed;

            int yil = (int)nudYil1.Value;
            int ay = cmbAy1.SelectedIndex + 1;

            var tahakkuklar = db.AidatTahakkuklar
                .Where(t => t.Yil == yil && t.Ay == ay && t.Durum != TahakkukDurumu.Odenmis)
                .Where(t => t.SonOdemeTarihi != null && t.SonOdemeTarihi < DateTime.Now)
                .ToList();

            if (tahakkuklar.Count == 0)
            {
                MessageBox.Show("Gecikme tazminat\u0131 uygulanacak tahakkuk bulunamad\u0131.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var aidatService = new AidatService();
            int uygulanan = 0;
            foreach (var t in tahakkuklar)
            {
                var sonuc = aidatService.GecikmeTazminatiHesapla(t.Id, oran);
                if (!sonuc.Contains("Hen\u00fcz") && !sonuc.Contains("bulunamad\u0131"))
                    uygulanan++;
            }

            MessageBox.Show($"{uygulanan} tahakkuka gecikme tazminat\u0131 uyguland\u0131.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadTahakkukListesi();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gecikme tazminat\u0131 uygulan\u0131rken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Tab 2 - Daire Ekstresi

    private TabPage CreateTab2()
    {
        var tab = new TabPage("Daire Ekstresi") { BackColor = Color.White };

        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        cmbDaire = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Font = new Font("Segoe UI", 10) };
        LoadDaireler();

        dtpEkstreBas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = new DateTime(DateTime.Now.Year, 1, 1) };
        dtpEkstreBit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 10), Value = DateTime.Now };

        var btnGoster = CreateButton("G\u00f6ster", Color.FromArgb(46, 134, 171));
        btnGoster.Click += (s, e) => LoadEkstre();

        pnlFilter.Controls.AddRange(new Control[]
        {
            new Label { Text = "Daire:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            cmbDaire,
            new Label { Text = "Ba\u015flang\u0131\u00e7:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            dtpEkstreBas,
            new Label { Text = "Biti\u015f:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            dtpEkstreBit,
            btnGoster
        });

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnPdf = CreateButton("PDF \u0130ndir", Color.FromArgb(231, 76, 60));
        btnPdf.Click += BtnEkstrePdf_Click;
        var btnExcel = CreateButton("Excel \u0130ndir", Color.FromArgb(39, 174, 96));
        btnExcel.Click += BtnEkstreExcel_Click;
        pnlButtons.Controls.AddRange(new Control[] { btnPdf, btnExcel });

        dgvEkstre = CreateGrid();
        dgvEkstre.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "D\u00f6nem", Name = "Donem" },
            new DataGridViewTextBoxColumn { HeaderText = "Aidat Tipi", Name = "AidatTipi" },
            new DataGridViewTextBoxColumn { HeaderText = "Tahakkuk", Name = "Tahakkuk", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6denen", Name = "Odenen", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Kalan", Name = "Kalan", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Gecikme Taz.", Name = "GecikmeTaz", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6deme Tarihi", Name = "OdemeTarihi" }
        });

        tab.Controls.Add(dgvEkstre);
        tab.Controls.Add(pnlButtons);
        tab.Controls.Add(pnlFilter);
        return tab;
    }

    private void LoadDaireler()
    {
        try
        {
            using var db = new AppDbContext();
            var daireler = db.Daireler.Include(d => d.Blok).OrderBy(d => d.Blok.Ad).ThenBy(d => d.DaireNo).ToList();
            cmbDaire.Items.Clear();
            foreach (var d in daireler)
                cmbDaire.Items.Add(new DaireComboItem { Id = d.Id, Display = $"{d.Blok.Ad} - Daire {d.DaireNo}" });
            if (cmbDaire.Items.Count > 0) cmbDaire.SelectedIndex = 0;
        }
        catch { }
    }

    private void LoadEkstre()
    {
        try
        {
            dgvEkstre.Rows.Clear();
            if (cmbDaire.SelectedItem is not DaireComboItem item) return;

            var service = new AidatService();
            var ekstre = service.DaireEkstresi(item.Id, dtpEkstreBas.Value, dtpEkstreBit.Value);

            foreach (var e in ekstre)
            {
                dgvEkstre.Rows.Add(e.Donem, e.AidatTipi, e.TahakkukTutar, e.OdenenTutar, e.KalanTutar, e.GecikmeTazminati, e.OdemeTarihi ?? "-");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ekstre y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private DataTable GetEkstreDataTable()
    {
        if (cmbDaire.SelectedItem is not DaireComboItem item) return new DataTable();
        var service = new RaporService();
        return service.DaireEkstreRaporu(item.Id, dtpEkstreBas.Value, dtpEkstreBit.Value);
    }

    private void BtnEkstrePdf_Click(object? sender, EventArgs e)
    {
        try
        {
            using var sfd = new SaveFileDialog { Filter = "PDF Dosyas\u0131|*.pdf", FileName = "DaireEkstresi.pdf" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var dt = GetEkstreDataTable();
                string[] basliklar = { "D\u00f6nem", "Aidat Tipi", "Tahakkuk", "\u00d6denen", "Kalan", "Gecikme Taz." };
                ExportHelper.ExportToPdf(dt, basliklar, "Daire Ekstresi", sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEkstreExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            using var sfd = new SaveFileDialog { Filter = "Excel Dosyas\u0131|*.xlsx", FileName = "DaireEkstresi.xlsx" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var dt = GetEkstreDataTable();
                string[] basliklar = { "D\u00f6nem", "Aidat Tipi", "Tahakkuk", "\u00d6denen", "Kalan", "Gecikme Taz." };
                ExportHelper.ExportToExcel(dt, basliklar, "Daire Ekstresi", sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Tab 3 - Gecikme Tazminati

    private TabPage CreateTab3()
    {
        var tab = new TabPage("Gecikme Tazminat\u0131") { BackColor = Color.White };

        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        nudYil3 = new NumericUpDown { Minimum = 2020, Maximum = 2050, Value = DateTime.Now.Year, Width = 80, Font = new Font("Segoe UI", 10) };
        cmbAy3 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = new Font("Segoe UI", 10) };
        cmbAy3.Items.AddRange(AyAdlari);
        cmbAy3.SelectedIndex = DateTime.Now.Month - 1;

        var btnListele = CreateButton("Listele", Color.FromArgb(46, 134, 171));
        btnListele.Click += (s, e) => LoadGecikmeListesi();

        pnlFilter.Controls.AddRange(new Control[]
        {
            new Label { Text = "Y\u0131l:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            nudYil3,
            new Label { Text = "Ay:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            cmbAy3,
            btnListele
        });

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnTumune = CreateButton("T\u00fcm\u00fcne Uygula", Color.FromArgb(231, 76, 60), 140);
        btnTumune.Click += BtnGecikmeTumuneUygula_Click;
        var btnSecilenlere = CreateButton("Se\u00e7ilenlere Uygula", Color.FromArgb(243, 156, 18), 160);
        btnSecilenlere.Click += BtnGecikmeSecilenler_Click;
        pnlButtons.Controls.AddRange(new Control[] { btnTumune, btnSecilenlere });

        dgvGecikme = CreateGrid();
        dgvGecikme.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewCheckBoxColumn { HeaderText = "Se\u00e7", Name = "Sec", Width = 40, ReadOnly = false },
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Blok", Name = "Blok" },
            new DataGridViewTextBoxColumn { HeaderText = "Daire", Name = "Daire" },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", Name = "Tutar", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6denen", Name = "Odenen", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Vade", Name = "Vade" },
            new DataGridViewTextBoxColumn { HeaderText = "Gecikme Taz.", Name = "GecikmeTaz", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } }
        });
        dgvGecikme.ReadOnly = false;
        dgvGecikme.Columns["Blok"]!.ReadOnly = true;
        dgvGecikme.Columns["Daire"]!.ReadOnly = true;
        dgvGecikme.Columns["Tutar"]!.ReadOnly = true;
        dgvGecikme.Columns["Odenen"]!.ReadOnly = true;
        dgvGecikme.Columns["Vade"]!.ReadOnly = true;
        dgvGecikme.Columns["GecikmeTaz"]!.ReadOnly = true;

        tab.Controls.Add(dgvGecikme);
        tab.Controls.Add(pnlButtons);
        tab.Controls.Add(pnlFilter);
        return tab;
    }

    private void LoadGecikmeListesi()
    {
        try
        {
            dgvGecikme.Rows.Clear();
            int yil = (int)nudYil3.Value;
            int ay = cmbAy3.SelectedIndex + 1;

            using var db = new AppDbContext();
            var list = db.AidatTahakkuklar
                .Include(t => t.Daire).ThenInclude(d => d.Blok)
                .Where(t => t.Yil == yil && t.Ay == ay && t.Durum != TahakkukDurumu.Odenmis)
                .Where(t => t.SonOdemeTarihi != null && t.SonOdemeTarihi < DateTime.Now)
                .OrderBy(t => t.Daire.Blok.Ad).ThenBy(t => t.Daire.DaireNo)
                .ToList();

            foreach (var t in list)
            {
                dgvGecikme.Rows.Add(false, t.Id, t.Daire.Blok.Ad, t.Daire.DaireNo,
                    t.Tutar, t.OdenenTutar, t.SonOdemeTarihi?.ToString("dd.MM.yyyy") ?? "-", t.GecikmeTazminati);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gecikme listesi y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private decimal GetFaizOrani()
    {
        try
        {
            using var db = new AppDbContext();
            var ayar = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "GecikmeFaizOrani");
            if (ayar != null && decimal.TryParse(ayar.Deger, out var oran))
                return oran;
        }
        catch { }
        return 2m;
    }

    private void BtnGecikmeTumuneUygula_Click(object? sender, EventArgs e)
    {
        try
        {
            var oran = GetFaizOrani();
            var service = new AidatService();
            int count = 0;
            foreach (DataGridViewRow row in dgvGecikme.Rows)
            {
                int id = (int)row.Cells["Id"].Value;
                var sonuc = service.GecikmeTazminatiHesapla(id, oran);
                if (!sonuc.Contains("Hen\u00fcz") && !sonuc.Contains("bulunamad\u0131"))
                    count++;
            }
            MessageBox.Show($"{count} tahakkuka gecikme tazminat\u0131 uyguland\u0131.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadGecikmeListesi();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gecikme tazminat\u0131 uygulan\u0131rken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGecikmeSecilenler_Click(object? sender, EventArgs e)
    {
        try
        {
            var oran = GetFaizOrani();
            var service = new AidatService();
            int count = 0;
            foreach (DataGridViewRow row in dgvGecikme.Rows)
            {
                bool secili = row.Cells["Sec"].Value is true;
                if (!secili) continue;
                int id = (int)row.Cells["Id"].Value;
                var sonuc = service.GecikmeTazminatiHesapla(id, oran);
                if (!sonuc.Contains("Hen\u00fcz") && !sonuc.Contains("bulunamad\u0131"))
                    count++;
            }
            MessageBox.Show($"{count} tahakkuka gecikme tazminat\u0131 uyguland\u0131.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadGecikmeListesi();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gecikme tazminat\u0131 uygulan\u0131rken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Size = new Size(width, 30),
            Margin = new Padding(3),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private class DaireComboItem
    {
        public int Id { get; set; }
        public string Display { get; set; } = "";
        public override string ToString() => Display;
    }

    #endregion
}

using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Dialogs;

namespace SiteYonetim.UI.Controls;

public class DaireControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    private TreeView treeBloklar = null!;
    private TabControl tabDaire = null!;

    // Tab 1 - Daire Bilgileri
    private Label lblDaireNo = null!;
    private Label lblKat = null!;
    private Label lblBlok = null!;
    private Label lblArsaPayi = null!;
    private Label lblTip = null!;

    // Tab 2 - Sakinler
    private DataGridView dgvSakinler = null!;

    // Tab 3 - Aidat Gecmisi
    private DataGridView dgvAidatGecmisi = null!;

    private int? _selectedDaireId;
    private int? _selectedBlokId;

    public DaireControl()
    {
        InitializeComponent();
        LoadTree();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(240, 244, 248);

        // Toolbar
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnBlokEkle = CreateButton("+ Blok Ekle", Color.FromArgb(46, 134, 171));
        btnBlokEkle.Click += BtnBlokEkle_Click;
        var btnDaireEkle = CreateButton("+ Daire Ekle", Color.FromArgb(46, 134, 171));
        btnDaireEkle.Click += BtnDaireEkle_Click;
        var btnDuzenle = CreateButton("\u270f D\u00fczenle", Color.FromArgb(243, 156, 18));
        btnDuzenle.Click += BtnDuzenle_Click;
        var btnSil = CreateButton("\ud83d\uddd1 Sil", Color.FromArgb(231, 76, 60));
        btnSil.Click += BtnSil_Click;

        toolbar.Controls.AddRange(new Control[] { btnBlokEkle, btnDaireEkle, btnDuzenle, btnSil });

        // Left - TreeView
        treeBloklar = new TreeView
        {
            Dock = DockStyle.Left,
            Width = 250,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(5),
            ItemHeight = 28
        };
        treeBloklar.AfterSelect += TreeBloklar_AfterSelect;

        // Right - TabControl
        tabDaire = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9)
        };

        // Tab 1: Daire Bilgileri
        var tabBilgiler = new TabPage("Daire Bilgileri") { BackColor = Color.White, Padding = new Padding(20) };
        var pnlBilgiler = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 5,
            AutoSize = true,
            Padding = new Padding(10)
        };
        pnlBilgiler.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        pnlBilgiler.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        lblDaireNo = AddInfoRow(pnlBilgiler, 0, "Daire No:");
        lblKat = AddInfoRow(pnlBilgiler, 1, "Kat:");
        lblBlok = AddInfoRow(pnlBilgiler, 2, "Blok:");
        lblArsaPayi = AddInfoRow(pnlBilgiler, 3, "Arsa Pay\u0131:");
        lblTip = AddInfoRow(pnlBilgiler, 4, "Tip:");
        tabBilgiler.Controls.Add(pnlBilgiler);

        // Tab 2: Sakinler
        var tabSakinler = new TabPage("Sakinler") { BackColor = Color.White };
        var pnlSakinButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            Padding = new Padding(10, 8, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };
        var btnSakinEkle = CreateButton("Sakin Ekle", Color.FromArgb(46, 134, 171));
        btnSakinEkle.Click += BtnSakinEkle_Click;
        var btnCikisYaptir = CreateButton("\u00c7\u0131k\u0131\u015f Yapt\u0131r", Color.FromArgb(231, 76, 60));
        btnCikisYaptir.Click += BtnCikisYaptir_Click;
        pnlSakinButtons.Controls.AddRange(new Control[] { btnSakinEkle, btnCikisYaptir });

        dgvSakinler = CreateGrid();
        dgvSakinler.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Ad\u0131 Soyad\u0131", Name = "AdSoyad" },
            new DataGridViewTextBoxColumn { HeaderText = "Tip", Name = "Tip" },
            new DataGridViewTextBoxColumn { HeaderText = "Telefon", Name = "Telefon" },
            new DataGridViewTextBoxColumn { HeaderText = "Giri\u015f Tarihi", Name = "GirisTarihi" },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", Name = "Durum" }
        });
        tabSakinler.Controls.Add(dgvSakinler);
        tabSakinler.Controls.Add(pnlSakinButtons);

        // Tab 3: Aidat Gecmisi
        var tabAidat = new TabPage("Aidat Ge\u00e7mi\u015fi") { BackColor = Color.White };
        dgvAidatGecmisi = CreateGrid();
        dgvAidatGecmisi.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "D\u00f6nem", Name = "Donem" },
            new DataGridViewTextBoxColumn { HeaderText = "Aidat Tipi", Name = "AidatTipi" },
            new DataGridViewTextBoxColumn { HeaderText = "Tahakkuk", Name = "Tahakkuk", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6denen", Name = "Odenen", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Kalan", Name = "Kalan", DefaultCellStyle = new DataGridViewCellStyle { Format = "\u20ba#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "\u00d6deme Tarihi", Name = "OdemeTarihi" }
        });
        tabAidat.Controls.Add(dgvAidatGecmisi);

        tabDaire.TabPages.AddRange(new TabPage[] { tabBilgiler, tabSakinler, tabAidat });

        var splitter = new Splitter { Dock = DockStyle.Left, Width = 5 };

        this.Controls.Add(tabDaire);
        this.Controls.Add(splitter);
        this.Controls.Add(treeBloklar);
        this.Controls.Add(toolbar);
    }

    private Label AddInfoRow(TableLayoutPanel panel, int row, string label)
    {
        var lbl = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 5)
        };
        var val = new Label
        {
            Text = "-",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(60, 60, 60),
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 5)
        };
        panel.Controls.Add(lbl, 0, row);
        panel.Controls.Add(val, 1, row);
        return val;
    }

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

    private Button CreateButton(string text, Color color)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Size = new Size(120, 30),
            Margin = new Padding(3),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void LoadTree()
    {
        try
        {
            treeBloklar.Nodes.Clear();
            using var db = new AppDbContext();
            var bloklar = db.Bloklar.Include(b => b.Daireler).OrderBy(b => b.Ad).ToList();

            foreach (var blok in bloklar)
            {
                var blokNode = new TreeNode(blok.Ad) { Tag = blok };
                foreach (var daire in blok.Daireler.OrderBy(d => d.DaireNo))
                {
                    var daireNode = new TreeNode($"Daire {daire.DaireNo}") { Tag = daire };
                    blokNode.Nodes.Add(daireNode);
                }
                treeBloklar.Nodes.Add(blokNode);
            }
            treeBloklar.ExpandAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"A\u011fa\u00e7 y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TreeBloklar_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is Daire daire)
        {
            _selectedDaireId = daire.Id;
            _selectedBlokId = daire.BlokId;
            LoadDaireBilgileri(daire.Id);
            LoadSakinler(daire.Id);
            LoadAidatGecmisi(daire.Id);
        }
        else if (e.Node?.Tag is Blok blok)
        {
            _selectedBlokId = blok.Id;
            _selectedDaireId = null;
            ClearDaireBilgileri();
        }
    }

    private void LoadDaireBilgileri(int daireId)
    {
        try
        {
            using var db = new AppDbContext();
            var daire = db.Daireler.Include(d => d.Blok).FirstOrDefault(d => d.Id == daireId);
            if (daire == null) return;

            lblDaireNo.Text = daire.DaireNo;
            lblKat.Text = daire.Kat.ToString();
            lblBlok.Text = daire.Blok.Ad;
            lblArsaPayi.Text = daire.ArsakPayi.ToString("0.##");
            lblTip.Text = daire.Tip.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Daire bilgileri y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearDaireBilgileri()
    {
        lblDaireNo.Text = "-";
        lblKat.Text = "-";
        lblBlok.Text = "-";
        lblArsaPayi.Text = "-";
        lblTip.Text = "-";
        dgvSakinler.Rows.Clear();
        dgvAidatGecmisi.Rows.Clear();
    }

    private void LoadSakinler(int daireId)
    {
        try
        {
            dgvSakinler.Rows.Clear();
            using var db = new AppDbContext();
            var sakinler = db.Sakinler.Where(s => s.DaireId == daireId).OrderByDescending(s => s.Aktif).ThenBy(s => s.Ad).ToList();

            foreach (var s in sakinler)
            {
                int idx = dgvSakinler.Rows.Add(s.Id, $"{s.Ad} {s.Soyad}", s.Tip.ToString(), s.Telefon ?? "-",
                    s.GirisTarihi.ToString("dd.MM.yyyy"), s.Aktif ? "Aktif" : "Pasif");
                if (!s.Aktif)
                    dgvSakinler.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sakinler y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadAidatGecmisi(int daireId)
    {
        try
        {
            dgvAidatGecmisi.Rows.Clear();
            var aidatService = new AidatService();
            var bitis = DateTime.Now;
            var baslangic = bitis.AddMonths(-12);
            var ekstre = aidatService.DaireEkstresi(daireId, baslangic, bitis);

            foreach (var e in ekstre)
            {
                dgvAidatGecmisi.Rows.Add(e.Donem, e.AidatTipi, e.TahakkukTutar, e.OdenenTutar, e.KalanTutar, e.OdemeTarihi ?? "-");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Aidat ge\u00e7mi\u015fi y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBlokEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new BlokEkleForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                db.Bloklar.Add(new Blok { Ad = dlg.BlokAd, Aciklama = dlg.BlokAciklama });
                db.SaveChanges();
                LoadTree();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Blok eklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDaireEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new DaireEkleForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                db.Daireler.Add(new Daire
                {
                    BlokId = dlg.SeciliBlokId,
                    DaireNo = dlg.DaireNo,
                    Kat = dlg.Kat,
                    ArsakPayi = dlg.ArsakPayi,
                    Tip = dlg.Tip
                });
                db.SaveChanges();
                LoadTree();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Daire eklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDuzenle_Click(object? sender, EventArgs e)
    {
        try
        {
            if (treeBloklar.SelectedNode?.Tag is Daire daire)
            {
                using var dlg = new DaireEkleForm(daire);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    using var db = new AppDbContext();
                    var entity = db.Daireler.Find(daire.Id);
                    if (entity != null)
                    {
                        entity.BlokId = dlg.SeciliBlokId;
                        entity.DaireNo = dlg.DaireNo;
                        entity.Kat = dlg.Kat;
                        entity.ArsakPayi = dlg.ArsakPayi;
                        entity.Tip = dlg.Tip;
                        db.SaveChanges();
                    }
                    LoadTree();
                    if (_selectedDaireId.HasValue)
                        LoadDaireBilgileri(_selectedDaireId.Value);
                    OnDataChanged?.Invoke();
                }
            }
            else if (treeBloklar.SelectedNode?.Tag is Blok blok)
            {
                using var dlg = new BlokEkleForm(blok);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    using var db = new AppDbContext();
                    var entity = db.Bloklar.Find(blok.Id);
                    if (entity != null)
                    {
                        entity.Ad = dlg.BlokAd;
                        entity.Aciklama = dlg.BlokAciklama;
                        db.SaveChanges();
                    }
                    LoadTree();
                    OnDataChanged?.Invoke();
                }
            }
            else
            {
                MessageBox.Show("L\u00fctfen d\u00fczenlemek i\u00e7in bir blok veya daire se\u00e7in.", "Uyar\u0131",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"D\u00fczenleme s\u0131ras\u0131nda hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSil_Click(object? sender, EventArgs e)
    {
        try
        {
            if (treeBloklar.SelectedNode?.Tag is Daire daire)
            {
                if (MessageBox.Show($"Daire {daire.DaireNo} silinecek. Emin misiniz?", "Silme Onay\u0131",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using var db = new AppDbContext();
                    var entity = db.Daireler.Find(daire.Id);
                    if (entity != null)
                    {
                        db.Daireler.Remove(entity);
                        db.SaveChanges();
                    }
                    _selectedDaireId = null;
                    LoadTree();
                    ClearDaireBilgileri();
                    OnDataChanged?.Invoke();
                }
            }
            else if (treeBloklar.SelectedNode?.Tag is Blok blok)
            {
                if (MessageBox.Show($"{blok.Ad} bloku ve t\u00fcm daireleri silinecek. Emin misiniz?", "Silme Onay\u0131",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using var db = new AppDbContext();
                    var entity = db.Bloklar.Include(b => b.Daireler).FirstOrDefault(b => b.Id == blok.Id);
                    if (entity != null)
                    {
                        db.Bloklar.Remove(entity);
                        db.SaveChanges();
                    }
                    _selectedBlokId = null;
                    _selectedDaireId = null;
                    LoadTree();
                    ClearDaireBilgileri();
                    OnDataChanged?.Invoke();
                }
            }
            else
            {
                MessageBox.Show("L\u00fctfen silmek i\u00e7in bir blok veya daire se\u00e7in.", "Uyar\u0131",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Silme s\u0131ras\u0131nda hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSakinEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!_selectedDaireId.HasValue)
            {
                MessageBox.Show("L\u00fctfen \u00f6nce bir daire se\u00e7in.", "Uyar\u0131",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SakinEkleForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                db.Sakinler.Add(new Sakin
                {
                    DaireId = _selectedDaireId.Value,
                    Ad = dlg.SakinAd,
                    Soyad = dlg.SakinSoyad,
                    TcKimlik = dlg.TcKimlik,
                    Telefon = dlg.Telefon,
                    Email = dlg.Email,
                    Tip = dlg.SakinTipiSecim,
                    GirisTarihi = dlg.GirisTarihi,
                    Aktif = true
                });
                db.SaveChanges();
                LoadSakinler(_selectedDaireId.Value);
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sakin eklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCikisYaptir_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvSakinler.CurrentRow == null) return;
            int sakinId = (int)dgvSakinler.CurrentRow.Cells["Id"].Value;

            string sakinAd = dgvSakinler.CurrentRow.Cells["AdSoyad"].Value?.ToString() ?? "";
            using var dlg = new CikisTarihiDialog(sakinAd);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                var sakin = db.Sakinler.Find(sakinId);
                if (sakin != null)
                {
                    sakin.Aktif = false;
                    sakin.CikisTarihi = dlg.CikisTarihi;
                    db.SaveChanges();
                }
                if (_selectedDaireId.HasValue)
                    LoadSakinler(_selectedDaireId.Value);
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"\u00c7\u0131k\u0131\u015f i\u015flemi s\u0131ras\u0131nda hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

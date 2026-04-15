using Microsoft.EntityFrameworkCore;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Dialogs;

namespace SiteYonetim.UI.Controls;

public class SakinControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    private TextBox txtArama = null!;
    private DataGridView dgvSakinler = null!;

    public SakinControl()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(240, 244, 248);

        // Toolbar
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        txtArama = new TextBox
        {
            PlaceholderText = "Ad/Soyad veya Daire No ile ara...",
            Width = 300,
            Height = 30,
            Font = new Font("Segoe UI", 10),
            Margin = new Padding(3)
        };
        txtArama.TextChanged += (s, e) => FilterData();

        var btnEkle = CreateButton("+ Sakin Ekle", Color.FromArgb(46, 134, 171));
        btnEkle.Click += BtnEkle_Click;
        var btnDuzenle = CreateButton("\u270f D\u00fczenle", Color.FromArgb(243, 156, 18));
        btnDuzenle.Click += BtnDuzenle_Click;
        var btnCikis = CreateButton("\ud83d\udeaa \u00c7\u0131k\u0131\u015f Yapt\u0131r", Color.FromArgb(231, 76, 60));
        btnCikis.Click += BtnCikis_Click;

        toolbar.Controls.AddRange(new Control[] { txtArama, btnEkle, btnDuzenle, btnCikis });

        // DataGridView
        dgvSakinler = new DataGridView
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
        dgvSakinler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
        dgvSakinler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvSakinler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvSakinler.ColumnHeadersHeight = 35;
        dgvSakinler.RowTemplate.Height = 28;
        dgvSakinler.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

        dgvSakinler.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Id", Name = "Id", Visible = false },
            new DataGridViewTextBoxColumn { HeaderText = "Ad\u0131 Soyad\u0131", Name = "AdSoyad", FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Daire", Name = "DaireNo", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Blok", Name = "Blok", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Tip", Name = "Tip", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Telefon", Name = "Telefon", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Giri\u015f Tarihi", Name = "GirisTarihi", FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", Name = "Durum", FillWeight = 10 }
        });

        this.Controls.Add(dgvSakinler);
        this.Controls.Add(toolbar);
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
            Size = new Size(130, 30),
            Margin = new Padding(3),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private List<Sakin> _allSakinler = new();

    private void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            _allSakinler = db.Sakinler
                .Include(s => s.Daire).ThenInclude(d => d.Blok)
                .OrderByDescending(s => s.Aktif).ThenBy(s => s.Ad).ThenBy(s => s.Soyad)
                .ToList();
            FilterData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sakinler y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FilterData()
    {
        var filter = txtArama.Text.Trim().ToLowerInvariant();
        dgvSakinler.Rows.Clear();

        var filtered = string.IsNullOrEmpty(filter)
            ? _allSakinler
            : _allSakinler.Where(s =>
                $"{s.Ad} {s.Soyad}".ToLowerInvariant().Contains(filter) ||
                s.Daire.DaireNo.ToLowerInvariant().Contains(filter)
            ).ToList();

        foreach (var s in filtered)
        {
            int idx = dgvSakinler.Rows.Add(
                s.Id,
                $"{s.Ad} {s.Soyad}",
                s.Daire.DaireNo,
                s.Daire.Blok.Ad,
                s.Tip.ToString(),
                s.Telefon ?? "-",
                s.GirisTarihi.ToString("dd.MM.yyyy"),
                s.Aktif ? "Aktif" : "Pasif"
            );

            if (!s.Aktif)
                dgvSakinler.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
        }
    }

    private void BtnEkle_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new SakinEkleForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                db.Sakinler.Add(new Sakin
                {
                    DaireId = dlg.SeciliDaireId,
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
                LoadData();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sakin eklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDuzenle_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvSakinler.CurrentRow == null)
            {
                MessageBox.Show("L\u00fctfen d\u00fczenlemek i\u00e7in bir sakin se\u00e7in.", "Uyar\u0131",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sakinId = (int)dgvSakinler.CurrentRow.Cells["Id"].Value;
            using var dbEdit = new AppDbContext();
            var sakinEntity = dbEdit.Sakinler.Find(sakinId);
            using var dlg = new SakinEkleForm(sakinEntity);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var db = new AppDbContext();
                var entity = db.Sakinler.Find(sakinId);
                if (entity != null)
                {
                    entity.DaireId = dlg.SeciliDaireId;
                    entity.Ad = dlg.SakinAd;
                    entity.Soyad = dlg.SakinSoyad;
                    entity.TcKimlik = dlg.TcKimlik;
                    entity.Telefon = dlg.Telefon;
                    entity.Email = dlg.Email;
                    entity.Tip = dlg.SakinTipiSecim;
                    entity.GirisTarihi = dlg.GirisTarihi;
                    db.SaveChanges();
                }
                LoadData();
                OnDataChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"D\u00fczenleme s\u0131ras\u0131nda hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCikis_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvSakinler.CurrentRow == null)
            {
                MessageBox.Show("L\u00fctfen \u00e7\u0131k\u0131\u015f yapt\u0131rmak i\u00e7in bir sakin se\u00e7in.", "Uyar\u0131",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                LoadData();
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

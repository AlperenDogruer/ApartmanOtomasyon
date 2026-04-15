using System.Data;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Helpers;
using SiteYonetim.Data;

namespace SiteYonetim.UI.Controls;

public class RaporlarControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    private ListBox lstRaporlar = null!;
    private Panel pnlParams = null!;
    private DataGridView dgvOnizleme = null!;

    // Param controls
    private NumericUpDown nudYil = null!;
    private ComboBox cmbAy = null!;
    private ComboBox cmbDaire = null!;
    private DateTimePicker dtpBas = null!;
    private DateTimePicker dtpBit = null!;

    // Param panels
    private Panel pnlBorclu = null!;
    private Panel pnlEkstre = null!;
    private Panel pnlMizan = null!;
    private Panel pnlGelirGider = null!;

    private DataTable? _currentData;
    private string[]? _currentBasliklar;
    private string? _currentRaporAdi;

    private static readonly string[] AyAdlari = { "Ocak", "\u015eubat", "Mart", "Nisan", "May\u0131s", "Haziran",
        "Temmuz", "A\u011fustos", "Eyl\u00fcl", "Ekim", "Kas\u0131m", "Aral\u0131k" };

    public RaporlarControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(240, 244, 248);

        // Left panel - report list
        lstRaporlar = new ListBox
        {
            Dock = DockStyle.Left,
            Width = 220,
            Font = new Font("Segoe UI", 11),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            ItemHeight = 35
        };
        lstRaporlar.Items.AddRange(new object[] { "Bor\u00e7lular Listesi", "Daire Ekstresi", "Y\u0131ll\u0131k Mizan", "Gelir-Gider Raporu" });
        lstRaporlar.SelectedIndexChanged += LstRaporlar_SelectedIndexChanged;

        var splitter = new Splitter { Dock = DockStyle.Left, Width = 5 };

        // Right panel
        var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        // Param area
        pnlParams = new Panel { Dock = DockStyle.Top, Height = 60 };

        CreateParamPanels();

        // Buttons
        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 50,
            Padding = new Padding(10, 10, 10, 5),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnOnizle = CreateButton("\u00d6nizle", Color.FromArgb(46, 134, 171));
        btnOnizle.Click += BtnOnizle_Click;
        var btnPdf = CreateButton("PDF \u0130ndir", Color.FromArgb(231, 76, 60));
        btnPdf.Click += BtnPdf_Click;
        var btnExcel = CreateButton("Excel \u0130ndir", Color.FromArgb(39, 174, 96));
        btnExcel.Click += BtnExcel_Click;
        pnlButtons.Controls.AddRange(new Control[] { btnOnizle, btnPdf, btnExcel });

        // DataGridView
        dgvOnizleme = CreateGrid();

        pnlRight.Controls.Add(dgvOnizleme);
        pnlRight.Controls.Add(pnlButtons);
        pnlRight.Controls.Add(pnlParams);

        this.Controls.Add(pnlRight);
        this.Controls.Add(splitter);
        this.Controls.Add(lstRaporlar);

        lstRaporlar.SelectedIndex = 0;
    }

    private void CreateParamPanels()
    {
        // Common controls
        nudYil = new NumericUpDown { Minimum = 2020, Maximum = 2050, Value = DateTime.Now.Year, Width = 80, Font = new Font("Segoe UI", 10) };
        cmbAy = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = new Font("Segoe UI", 10) };
        cmbAy.Items.AddRange(AyAdlari);
        cmbAy.SelectedIndex = DateTime.Now.Month - 1;

        cmbDaire = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Font = new Font("Segoe UI", 10) };
        LoadDaireler();

        dtpBas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Font = new Font("Segoe UI", 10), Value = new DateTime(DateTime.Now.Year, 1, 1) };
        dtpBit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Font = new Font("Segoe UI", 10), Value = DateTime.Now };

        // Panel 1: Borclular
        pnlBorclu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5) };
        pnlBorclu.Controls.AddRange(new Control[]
        {
            new Label { Text = "Y\u0131l:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            nudYil,
            new Label { Text = "Ay:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            cmbAy
        });

        // Panel 2: Ekstre
        pnlEkstre = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), Visible = false };
        pnlEkstre.Controls.AddRange(new Control[]
        {
            new Label { Text = "Daire:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            cmbDaire,
            new Label { Text = "Ba\u015f:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            dtpBas,
            new Label { Text = "Bit:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            dtpBit
        });

        // Panel 3: Mizan (reuses nudYil)
        pnlMizan = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), Visible = false };
        // We need a separate NumericUpDown for mizan since controls can't be in two parents
        var nudYilMizan = new NumericUpDown { Minimum = 2020, Maximum = 2050, Value = DateTime.Now.Year, Width = 80, Font = new Font("Segoe UI", 10), Tag = "mizan" };
        pnlMizan.Controls.AddRange(new Control[]
        {
            new Label { Text = "Y\u0131l:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            nudYilMizan
        });

        // Panel 4: Gelir-Gider
        pnlGelirGider = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), Visible = false };
        var dtpGGBas = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Font = new Font("Segoe UI", 10), Value = new DateTime(DateTime.Now.Year, 1, 1), Tag = "gg_bas" };
        var dtpGGBit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Font = new Font("Segoe UI", 10), Value = DateTime.Now, Tag = "gg_bit" };
        pnlGelirGider.Controls.AddRange(new Control[]
        {
            new Label { Text = "Ba\u015flang\u0131\u00e7:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(0, 5, 0, 0) },
            dtpGGBas,
            new Label { Text = "Biti\u015f:", AutoSize = true, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 5, 0, 0) },
            dtpGGBit
        });

        pnlParams.Controls.AddRange(new Control[] { pnlBorclu, pnlEkstre, pnlMizan, pnlGelirGider });
    }

    private void LoadDaireler()
    {
        try
        {
            using var db = new AppDbContext();
            var daireler = db.Daireler.Include(d => d.Blok).OrderBy(d => d.Blok.Ad).ThenBy(d => d.DaireNo).ToList();
            cmbDaire.Items.Clear();
            foreach (var d in daireler)
                cmbDaire.Items.Add(new DaireItem { Id = d.Id, Display = $"{d.Blok.Ad} - Daire {d.DaireNo}" });
            if (cmbDaire.Items.Count > 0) cmbDaire.SelectedIndex = 0;
        }
        catch { }
    }

    private void LstRaporlar_SelectedIndexChanged(object? sender, EventArgs e)
    {
        pnlBorclu.Visible = lstRaporlar.SelectedIndex == 0;
        pnlEkstre.Visible = lstRaporlar.SelectedIndex == 1;
        pnlMizan.Visible = lstRaporlar.SelectedIndex == 2;
        pnlGelirGider.Visible = lstRaporlar.SelectedIndex == 3;
    }

    private void BtnOnizle_Click(object? sender, EventArgs e)
    {
        try
        {
            var service = new RaporService();
            dgvOnizleme.Columns.Clear();
            dgvOnizleme.DataSource = null;

            switch (lstRaporlar.SelectedIndex)
            {
                case 0: // Borclular
                    _currentData = service.BorcluListesiRaporu((int)nudYil.Value, cmbAy.SelectedIndex + 1);
                    _currentBasliklar = new[] { "Blok", "Daire No", "Sakin", "Bor\u00e7 Tutar\u0131", "Gecikme Tazminat\u0131", "Durum" };
                    _currentRaporAdi = "Bor\u00e7lular Listesi";
                    break;

                case 1: // Ekstre
                    if (cmbDaire.SelectedItem is not DaireItem item)
                    {
                        MessageBox.Show("L\u00fctfen bir daire se\u00e7in.", "Uyar\u0131", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    _currentData = service.DaireEkstreRaporu(item.Id, dtpBas.Value, dtpBit.Value);
                    _currentBasliklar = new[] { "D\u00f6nem", "Aidat Tipi", "Tahakkuk", "\u00d6denen", "Kalan", "Gecikme Taz." };
                    _currentRaporAdi = "Daire Ekstresi";
                    break;

                case 2: // Mizan
                    var nudMizan = FindControlByTag<NumericUpDown>(pnlMizan, "mizan");
                    _currentData = service.YillikMizanRaporu((int)(nudMizan?.Value ?? DateTime.Now.Year));
                    _currentBasliklar = new[] { "Ay", "Gelir", "Gider", "Net" };
                    _currentRaporAdi = "Y\u0131ll\u0131k Mizan";
                    break;

                case 3: // Gelir-Gider
                    var dtpGGBas = FindControlByTag<DateTimePicker>(pnlGelirGider, "gg_bas");
                    var dtpGGBit = FindControlByTag<DateTimePicker>(pnlGelirGider, "gg_bit");
                    _currentData = service.GelirGiderRaporu(dtpGGBas?.Value ?? DateTime.Now.AddMonths(-1), dtpGGBit?.Value ?? DateTime.Now);
                    _currentBasliklar = new[] { "T\u00fcr", "Tarih", "A\u00e7\u0131klama", "Kategori", "Tutar" };
                    _currentRaporAdi = "Gelir-Gider Raporu";
                    break;
            }

            if (_currentData != null)
            {
                dgvOnizleme.DataSource = _currentData;
                // Style decimal columns
                foreach (DataGridViewColumn col in dgvOnizleme.Columns)
                {
                    if (col.ValueType == typeof(decimal))
                    {
                        col.DefaultCellStyle.Format = "\u20ba#,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Rapor y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentData == null || _currentBasliklar == null)
            {
                MessageBox.Show("\u00d6nce raporu \u00f6nizleyin.", "Uyar\u0131", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var sfd = new SaveFileDialog { Filter = "PDF Dosyas\u0131|*.pdf", FileName = $"{_currentRaporAdi}.pdf" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportHelper.ExportToPdf(_currentData, _currentBasliklar, _currentRaporAdi ?? "Rapor", sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentData == null || _currentBasliklar == null)
            {
                MessageBox.Show("\u00d6nce raporu \u00f6nizleyin.", "Uyar\u0131", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var sfd = new SaveFileDialog { Filter = "Excel Dosyas\u0131|*.xlsx", FileName = $"{_currentRaporAdi}.xlsx" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportHelper.ExportToExcel(_currentData, _currentBasliklar, _currentRaporAdi ?? "Rapor", sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel olu\u015fturulurken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private T? FindControlByTag<T>(Control parent, string tag) where T : Control
    {
        foreach (Control c in parent.Controls)
        {
            if (c is T tc && tc.Tag?.ToString() == tag) return tc;
            var found = FindControlByTag<T>(c, tag);
            if (found != null) return found;
        }
        return null;
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

    private class DaireItem
    {
        public int Id { get; set; }
        public string Display { get; set; } = "";
        public override string ToString() => Display;
    }
}

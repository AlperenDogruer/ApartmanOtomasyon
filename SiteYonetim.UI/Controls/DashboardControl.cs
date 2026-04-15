using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.DTOs;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;

namespace SiteYonetim.UI.Controls;

public class DashboardControl : UserControl, IRefreshable
{
    public event Action? OnDataChanged;

    private FlowLayoutPanel pnlCards = null!;
    private DataGridView dgvBorclular = null!;
    private Label lblCardTahakkuk = null!;
    private Label lblCardTahsilat = null!;
    private Label lblCardBorclu = null!;
    private Label lblCardKasa = null!;

    public DashboardControl()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Padding = new Padding(10);

        // Cards panel
        pnlCards = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 120,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(5)
        };

        lblCardTahakkuk = CreateCard("Bu Ay Tahakkuk", "₺0,00", Color.FromArgb(46, 134, 171));
        lblCardTahsilat = CreateCard("Bu Ay Tahsilat", "₺0,00", Color.FromArgb(39, 174, 96));
        lblCardBorclu = CreateCard("Bor\u00e7lu Daire", "0", Color.FromArgb(243, 156, 18));
        lblCardKasa = CreateCard("Kasa Bakiyesi", "₺0,00", Color.FromArgb(30, 58, 95));

        // Title label
        var lblTitle = new Label
        {
            Text = "Bu Ay Borçlu Daireler",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Dock = DockStyle.Top,
            Height = 35,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5, 5, 0, 0)
        };

        // DataGridView
        dgvBorclular = new DataGridView
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
        dgvBorclular.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
        dgvBorclular.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvBorclular.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvBorclular.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
        dgvBorclular.ColumnHeadersHeight = 35;
        dgvBorclular.DefaultCellStyle.Padding = new Padding(5);
        dgvBorclular.RowTemplate.Height = 30;
        dgvBorclular.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

        dgvBorclular.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Blok", Name = "Blok" },
            new DataGridViewTextBoxColumn { HeaderText = "Daire No", Name = "DaireNo" },
            new DataGridViewTextBoxColumn { HeaderText = "Sakin Ad\u0131", Name = "SakinAd" },
            new DataGridViewTextBoxColumn { HeaderText = "Bor\u00e7 Tutar\u0131", Name = "BorcTutari", DefaultCellStyle = new DataGridViewCellStyle { Format = "₺#,##0.00", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", Name = "Durum" }
        });

        this.Controls.Add(dgvBorclular);
        this.Controls.Add(lblTitle);
        this.Controls.Add(pnlCards);
    }

    private Label CreateCard(string title, string value, Color accentColor)
    {
        var card = new Panel
        {
            Width = 230,
            Height = 100,
            Margin = new Padding(8),
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.Gray,
            Dock = DockStyle.Top,
            Height = 25,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var lblValue = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = accentColor,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var accentLine = new Panel
        {
            Dock = DockStyle.Top,
            Height = 3,
            BackColor = accentColor
        };

        card.Controls.Add(lblValue);
        card.Controls.Add(lblTitle);
        card.Controls.Add(accentLine);
        pnlCards.Controls.Add(card);

        return lblValue;
    }

    private void LoadData()
    {
        try
        {
            var gelirGiderService = new GelirGiderService();
            var aidatService = new AidatService();
            var now = DateTime.Now;

            // Ozet
            var ozet = gelirGiderService.AylikOzet(now.Year, now.Month);
            lblCardTahakkuk.Text = ozet.ToplamTahakkuk.ToString("₺#,##0.00");
            lblCardTahsilat.Text = ozet.AidatTahsilati.ToString("₺#,##0.00");
            lblCardBorclu.Text = ozet.BorcluDaireSayisi.ToString();

            // Kasa - all time net
            var (gelir, gider, net) = gelirGiderService.KasaDurumu(new DateTime(2000, 1, 1), DateTime.Now);
            lblCardKasa.Text = net.ToString("₺#,##0.00");
            lblCardKasa.ForeColor = net >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(231, 76, 60);

            // Borclular
            var borclular = aidatService.AylikBorcluListesi(now.Year, now.Month);
            dgvBorclular.Rows.Clear();
            foreach (var b in borclular)
            {
                int idx = dgvBorclular.Rows.Add(b.BlokAd, b.DaireNo, b.SakinAd, b.BorcTutari, b.Durum);
                var row = dgvBorclular.Rows[idx];
                if (b.Durum == "\u00d6denmemi\u015f")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                }
                else if (b.Durum == "K\u0131smi \u00d6deme")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 231);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(243, 156, 18);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Veriler y\u00fcklenirken hata olu\u015ftu: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

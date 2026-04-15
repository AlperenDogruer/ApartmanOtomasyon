namespace SiteYonetim.UI.Dialogs;

public class CikisTarihiDialog : Form
{
    private Label lblSakinAd = null!;
    private DateTimePicker dtpCikisTarihi = null!;
    private Button btnOnayla = null!;
    private Button btnIptal = null!;

    public DateTime CikisTarihi => dtpCikisTarihi.Value;

    public CikisTarihiDialog(string sakinAdSoyad)
    {
        InitializeComponent(sakinAdSoyad);
    }

    private void InitializeComponent(string sakinAdSoyad)
    {
        this.Text = "Cikis Tarihi Belirle";
        this.Size = new Size(400, 250);
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
            Text = "Sakin Cikis Islemleri",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblBaslik);

        // Sakin name
        var lblSakinLabel = new Label
        {
            Text = "Sakin:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(20, 68),
            AutoSize = true
        };

        lblSakinAd = new Label
        {
            Text = sakinAdSoyad,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(130, 66),
            AutoSize = true
        };

        // Cikis Tarihi
        var lblCikis = new Label
        {
            Text = "Cikis Tarihi:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(30, 58, 95),
            Location = new Point(20, 108),
            AutoSize = true
        };

        dtpCikisTarihi = new DateTimePicker
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(130, 105),
            Size = new Size(230, 28),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today
        };

        // Buttons
        btnOnayla = new Button
        {
            Text = "Onayla",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 36),
            Location = new Point(140, 160),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(46, 134, 171),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnOnayla.FlatAppearance.BorderSize = 0;
        btnOnayla.Click += (s, e) =>
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        btnIptal = new Button
        {
            Text = "Iptal",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 36),
            Location = new Point(250, 160),
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
        this.Controls.Add(lblSakinLabel);
        this.Controls.Add(lblSakinAd);
        this.Controls.Add(lblCikis);
        this.Controls.Add(dtpCikisTarihi);
        this.Controls.Add(btnOnayla);
        this.Controls.Add(btnIptal);

        this.AcceptButton = btnOnayla;
        this.CancelButton = btnIptal;
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using ClientSinhVien.Controls;
using ClientSinhVien.Helpers;
using ClientSinhVien.Services;

namespace ClientSinhVien.Panels
{
    /// <summary>
    /// File: LogPanel.cs
    /// Chức năng: Khu vực hiển thị Lịch sử hoạt động (Log) phía dưới cùng.
    /// Nhiệm vụ:
    /// - Hiển thị các thay đổi thao tác (Thêm, Xóa, Cập nhật) để người dùng theo dõi theo thời gian thực.
    /// - Cho phép mở rộng / thu gọn cửa sổ Log để tối ưu không gian hiển thị bảng.
    /// - Có nút xóa (Clear) toàn bộ Log.
    /// </summary>
    public class LogPanel : Panel
    {
        private RichTextBox _rtb;
        private FlatButton  _btnToggle;
        private FlatButton  _btnClear;
        private Panel       _header;
        private bool        _expanded = true;

        private const int EXPANDED_H  = 160;
        private const int COLLAPSED_H = 32;

        public LogPanel()
        {
            BackColor = ColorPalette.Card;
            Height    = EXPANDED_H;
            Dock      = DockStyle.Bottom;
            Build();
        }

        private void Build()
        {
            // ── Header bar ────────────────────────────────────────────────────
            _header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                BackColor = ColorPalette.Header,
            };
            _header.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(ColorPalette.Border), 0, 0, _header.Width, 0);

            var lblTitle = new Label
            {
                Text      = "📋  Lịch sử hoạt động",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ColorPalette.SubText,
                Location  = new Point(12, 7),
                AutoSize  = true,
                BackColor = Color.Transparent,
            };

            _btnClear = new FlatButton
            {
                Text     = "Xóa log",
                BgColor  = Color.FromArgb(50, 50, 80),
                TxtColor = ColorPalette.SubText,
                Radius   = 6,
                Font     = new Font("Segoe UI", 8f),
                Size     = new Size(70, 22),
                Anchor   = AnchorStyles.Top | AnchorStyles.Right,
                Cursor   = Cursors.Hand,
            };
            _btnToggle = new FlatButton
            {
                Text     = "Thu gọn ▲",
                BgColor  = Color.FromArgb(50, 50, 80),
                TxtColor = ColorPalette.SubText,
                Radius   = 6,
                Font     = new Font("Segoe UI", 8f),
                Size     = new Size(90, 22),
                Anchor   = AnchorStyles.Top | AnchorStyles.Right,
                Cursor   = Cursors.Hand,
            };

            PosHeaderButtons();
            _header.Resize += (s, e) => PosHeaderButtons();

            _btnToggle.Click += (s, e) => Toggle();
            _btnClear.Click  += (s, e) => ClearLog();

            _header.Controls.Add(lblTitle);
            _header.Controls.Add(_btnClear);
            _header.Controls.Add(_btnToggle);

            // ── RichTextBox ───────────────────────────────────────────────────
            _rtb = new RichTextBox
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorPalette.Card,
                ForeColor = ColorPalette.Text,
                Font      = new Font("Consolas", 9f),
                ReadOnly  = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
            };

            Controls.Add(_rtb);
            Controls.Add(_header);
        }

        private void PosHeaderButtons()
        {
            int right = _header.Width - 8;
            _btnToggle.Location = new Point(right - 90, 5);
            _btnClear.Location  = new Point(right - 90 - 6 - 70, 5);
        }

        // ── Toggle collapse/expand ────────────────────────────────────────────
        private void Toggle()
        {
            _expanded      = !_expanded;
            Height         = _expanded ? EXPANDED_H : COLLAPSED_H;
            _rtb.Visible   = _expanded;
            _btnToggle.Text = _expanded ? "Thu gọn ▲" : "Mở rộng ▼";
        }

        // ── Xóa log ───────────────────────────────────────────────────────────
        private void ClearLog() => _rtb.Clear();

        // ── Thêm dòng log (thread-safe) ───────────────────────────────────────
        public void AppendEntry(LogEntry entry)
        {
            if (InvokeRequired) { Invoke((Action)(() => AppendEntry(entry))); return; }

            _rtb.SelectionStart  = _rtb.TextLength;
            _rtb.SelectionLength = 0;
            _rtb.SelectionColor  = entry.Color;
            _rtb.AppendText(entry.Message + Environment.NewLine);
            _rtb.ScrollToCaret();

            // Tự mở rộng nếu đang thu gọn
            if (!_expanded) Toggle();
        }
    }
}

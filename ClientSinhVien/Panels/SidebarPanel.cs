using System;
using System.Drawing;
using System.Windows.Forms;
using ClientSinhVien.Controls;
using ClientSinhVien.Helpers;

namespace ClientSinhVien.Panels
{
    /// <summary>
    /// File: SidebarPanel.cs
    /// Chức năng: Khu vực điều khiển bên trái (Sidebar) của giao diện quản lý.
    /// Nhiệm vụ:
    /// - Hiển thị logo, tiêu đề phần mềm.
    /// - Quản lý phần Kết nối Máy chủ (Nhập IP, Port, Nút Kết nối/Ngắt kết nối).
    /// - Các ô nhập liệu thông tin Sinh viên (MSSV, Họ tên, Lớp, Điểm).
    /// - Các nút bấm thao tác dữ liệu: Thêm mới, Cập nhật, Xóa, Xóa Form (làm sạch ô nhập).
    /// </summary>
    public class SidebarPanel : Panel
    {
        // ── Kết nối ───────────────────────────────────────────────────────────
        public FlatTextBox  TxtHost    { get; private set; }
        public FlatTextBox  TxtPort    { get; private set; }
        public FlatButton   BtnConnect { get; private set; }

        // ── Fields nhập SV ────────────────────────────────────────────────────
        public FlatTextBox  TxtMSSV    { get; private set; }
        public FlatTextBox  TxtHoTen   { get; private set; }
        public FlatTextBox  TxtLop     { get; private set; }
        public FlatTextBox  TxtDiem    { get; private set; }

        // ── Buttons CRUD ──────────────────────────────────────────────────────
        public FlatButton   BtnThem    { get; private set; }
        public FlatButton   BtnCapNhat { get; private set; }
        public FlatButton   BtnXoa     { get; private set; }
        public FlatButton   BtnXoaForm { get; private set; }

        // ── Status kết nối ────────────────────────────────────────────────────
        public Label        LblStatus  { get; private set; }

        public SidebarPanel()
        {
            BackColor = ColorPalette.Sidebar;
            Width     = 260;
            Dock      = DockStyle.Left;
            Padding   = new Padding(16);
            Build();
        }

        private void Build()
        {
            int y = 16;

            // ── Logo / tiêu đề ─────────────────────────────────────────────
            var logo = new Label
            {
                Text      = "🎓  Quản Lý\nSinh Viên",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ColorPalette.Accent,
                Location  = new Point(16, y),
                Size      = new Size(228, 52),
                BackColor = Color.Transparent,
            };
            Controls.Add(logo);
            y += 62;

            // ── Divider ────────────────────────────────────────────────────
            y = AddDivider(y, "KẾT NỐI MÁY CHỦ");

            // ── Host ───────────────────────────────────────────────────────
            y = AddLabel("Địa chỉ máy chủ", y);
            TxtHost = AddTextBox("127.0.0.1", ref y);

            // ── Port ───────────────────────────────────────────────────────
            y = AddLabel("Cổng", y);
            TxtPort = AddTextBox("5000", ref y);

            // ── Connect button ─────────────────────────────────────────────
            BtnConnect = AddButton("🔌  Kết nối", ColorPalette.Accent, ref y);

            // ── Status ────────────────────────────────────────────────────
            LblStatus = new Label
            {
                Text      = "● Chưa kết nối",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = ColorPalette.Danger,
                Location  = new Point(16, y),
                Size      = new Size(228, 20),
                BackColor = Color.Transparent,
            };
            Controls.Add(LblStatus);
            y += 26;

            // ── Divider ────────────────────────────────────────────────────
            y = AddDivider(y, "THÔNG TIN SINH VIÊN");

            y = AddLabel("Mã số sinh viên", y);
            TxtMSSV = AddTextBox("", ref y);

            // ── Họ tên ────────────────────────────────────────────────────
            y = AddLabel("Họ và tên", y);
            TxtHoTen = AddTextBox("", ref y);

            // ── Lớp ──────────────────────────────────────────────────────
            y = AddLabel("Lớp", y);
            TxtLop = AddTextBox("", ref y);

            // ── Điểm ─────────────────────────────────────────────────────
            y = AddLabel("Điểm trung bình", y);
            TxtDiem = AddTextBox("", ref y);

            // ── Divider ────────────────────────────────────────────────────
            y = AddDivider(y, "THAO TÁC");

            // ── CRUD buttons ───────────────────────────────────────────────
            BtnThem    = AddButton("➕  Thêm",       ColorPalette.Success, ref y);
            BtnCapNhat = AddButton("✏️  Cập nhật",   ColorPalette.Accent,  ref y);
            BtnXoa     = AddButton("🗑️  Xóa",        ColorPalette.Danger,  ref y);
            BtnXoaForm = AddButton("🧹  Xóa form",   Color.FromArgb(50, 50, 80), ref y);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private int AddLabel(string text, int y)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = ColorPalette.SubText,
                Location  = new Point(16, y),
                Size      = new Size(228, 18),
                BackColor = Color.Transparent,
            };
            Controls.Add(lbl);
            return y + 20;
        }

        private FlatTextBox AddTextBox(string placeholder, ref int y)
        {
            var tb = new FlatTextBox
            {
                Location = new Point(16, y),
                Size     = new Size(228, 32),
                BgColor  = ColorPalette.InputBg,
                TxtColor = ColorPalette.Text,
                BdColor  = ColorPalette.Border,
                Text     = placeholder,
            };
            Controls.Add(tb);
            y += 38;
            return tb;
        }

        private FlatButton AddButton(string text, Color color, ref int y)
        {
            var btn = new FlatButton
            {
                Text     = text,
                BgColor  = color,
                TxtColor = Color.White,
                Font     = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(16, y),
                Size     = new Size(228, 34),
                Cursor   = Cursors.Hand,
            };
            Controls.Add(btn);
            y += 40;
            return btn;
        }

        private int AddDivider(int y, string label)
        {
            var line = new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ColorPalette.SubText,
                Location  = new Point(16, y + 4),
                Size      = new Size(228, 16),
                BackColor = Color.Transparent,
            };
            Controls.Add(line);
            var sep = new Panel
            {
                Location  = new Point(16, y + 22),
                Size      = new Size(228, 1),
                BackColor = ColorPalette.Border,
            };
            Controls.Add(sep);
            return y + 30;
        }

        // ── Điền dữ liệu vào form ─────────────────────────────────────────────
        public void Fill(string mssv, string hoTen, string lop, string diem)
        {
            TxtMSSV.Text  = mssv;
            TxtHoTen.Text = hoTen;
            TxtLop.Text   = lop;
            TxtDiem.Text  = diem;
        }

        // ── Xóa toàn bộ form ──────────────────────────────────────────────────
        public void Clear()
        {
            TxtMSSV.Text   = "";
            TxtHoTen.Text  = "";
            TxtLop.Text    = "";
            TxtDiem.Text   = "";
        }

        // ── Cập nhật trạng thái kết nối ──────────────────────────────────────
        public void SetConnected(bool connected)
        {
            if (InvokeRequired) { Invoke((Action<bool>)SetConnected, connected); return; }
            LblStatus.Text      = connected ? "● Đã kết nối" : "● Chưa kết nối";
            LblStatus.ForeColor = connected ? ColorPalette.Success : ColorPalette.Danger;
            BtnConnect.Text     = connected ? "🔌  Ngắt kết nối" : "🔌  Kết nối";
        }
    }
}

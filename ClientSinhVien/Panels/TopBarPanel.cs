using System;
using System.Drawing;
using System.Windows.Forms;
using ClientSinhVien.Controls;
using ClientSinhVien.Helpers;

namespace ClientSinhVien.Panels
{
    /// <summary>
    /// File: TopBarPanel.cs
    /// Chức năng: Khu vực thanh công cụ phía trên (Top bar) của giao diện quản lý.
    /// Nhiệm vụ:
    /// - Chứa các ô tìm kiếm: tìm theo từ khóa (tên/mã), tìm theo điểm, tìm theo lớp.
    /// - Cung cấp các nút bấm: Tải lại danh sách (Load), Sắp xếp theo Lớp (Sort), Xuất ra file Excel (Export).
    /// - Có thiết kế reponsive (tự động căn lề sang phải khi co giãn cửa sổ).
    /// </summary>
    public class TopBarPanel : Panel
    {
        // ── Search ────────────────────────────────────────────────────────────
        public FlatTextBox TxtSearch      { get; private set; }   // tìm chung
        public FlatTextBox TxtSearchDiem  { get; private set; }   // Yêu cầu 6a
        public FlatTextBox TxtSearchLop   { get; private set; }   // Yêu cầu 6b

        // ── Buttons ───────────────────────────────────────────────────────────
        public FlatButton  BtnLoad        { get; private set; }
        public FlatButton  BtnSort        { get; private set; }   // Yêu cầu 2
        public FlatButton  BtnExcel       { get; private set; }   // Yêu cầu 3

        // ── Sort state toggle ─────────────────────────────────────────────────
        public bool SortAscending { get; private set; } = true;

        public TopBarPanel()
        {
            BackColor = ColorPalette.Header;
            Height    = 56;
            Dock      = DockStyle.Top;
            Build();
        }

        private void Build()
        {
            // ── Tiêu đề ───────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text      = "DANH SÁCH SINH VIÊN",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorPalette.Text,
                Location  = new Point(16, 18),
                AutoSize  = true,
                BackColor = Color.Transparent,
            };
            Controls.Add(lblTitle);

            // ── Tìm kiếm chung ────────────────────────────────────────────
            TxtSearch = new FlatTextBox
            {
                Size    = new Size(160, 30),
                BgColor = ColorPalette.InputBg,
                TxtColor= ColorPalette.Text,
                BdColor = ColorPalette.Border,
                PlaceholderText = "🔍  Tìm kiếm...",
                Text    = "",
            };
            Controls.Add(TxtSearch);

            // ── Tìm theo điểm ─────────────────────────────────────────────
            TxtSearchDiem = new FlatTextBox
            {
                Size    = new Size(130, 30),
                BgColor = ColorPalette.InputBg,
                TxtColor= ColorPalette.Text,
                BdColor = ColorPalette.Border,
                PlaceholderText = "Điểm: 7.0-9.0",
                Text    = "",
            };
            Controls.Add(TxtSearchDiem);

            // ── Tìm theo lớp ──────────────────────────────────────────────
            TxtSearchLop = new FlatTextBox
            {
                Size    = new Size(130, 30),
                BgColor = ColorPalette.InputBg,
                TxtColor= ColorPalette.Text,
                BdColor = ColorPalette.Border,
                PlaceholderText = "Tìm theo lớp...",
                Text    = "",
            };
            Controls.Add(TxtSearchLop);

            // ── Nút Tải lại ───────────────────────────────────────────────
            BtnLoad = new FlatButton
            {
                Text    = "↺  Tải lại",
                BgColor = Color.FromArgb(50, 50, 80),
                Font    = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size    = new Size(80, 30),
                Cursor  = Cursors.Hand,
            };
            Controls.Add(BtnLoad);

            // ── Nút Sort ──────────────────────────────────────────────────
            BtnSort = new FlatButton
            {
                Text    = "Lớp ↑",
                BgColor = Color.FromArgb(50, 50, 80),
                Font    = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size    = new Size(70, 30),
                Cursor  = Cursors.Hand,
            };
            BtnSort.Click += (s, e) =>
            {
                SortAscending   = !SortAscending;
                BtnSort.Text    = SortAscending ? "Lớp ↑" : "Lớp ↓";
            };
            Controls.Add(BtnSort);

            // ── Nút Xuất Excel ────────────────────────────────────────────
            BtnExcel = new FlatButton
            {
                Text    = "📊  Excel",
                BgColor = Color.FromArgb(34, 120, 70),
                Font    = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size    = new Size(80, 30),
                Cursor  = Cursors.Hand,
            };
            Controls.Add(BtnExcel);

            // ── Layout khi resize ─────────────────────────────────────────
            Resize += (s, e) => LayoutControls();
            LayoutControls();
        }

        private void LayoutControls()
        {
            int right = Width - 12;
            int cy    = (Height - 30) / 2;

            BtnExcel.Location       = new Point(right - 80,  cy);
            BtnSort.Location        = new Point(right - 80  - 4 - 70,  cy);
            BtnLoad.Location        = new Point(right - 80  - 4 - 70  - 4 - 80, cy);
            TxtSearchLop.Location   = new Point(BtnLoad.Left - 4 - 130, cy);
            TxtSearchDiem.Location  = new Point(TxtSearchLop.Left - 4 - 130, cy);
            TxtSearch.Location      = new Point(TxtSearchDiem.Left - 4 - 160, cy);
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClientSinhVien.Helpers;
using ClientSinhVien.Models;
using ClientSinhVien.Services;

namespace ClientSinhVien.Controls
{
    /// <summary>
    /// Card thống kê hiển thị khi tìm kiếm theo Lớp (Yêu cầu 6b).
    /// Hiển thị: tên lớp, tổng SV, ĐTB, phân bố xếp loại.
    /// Ẩn khi tìm kiếm thông thường.
    /// </summary>
    public class StatCard : Panel
    {
        private Label _lblLine1;
        private Label _lblLine2;

        public StatCard()
        {
            BackColor   = ColorPalette.Card;
            Height      = 56;
            Dock        = DockStyle.Top;
            Padding     = new Padding(16, 8, 16, 8);
            Visible     = false;

            _lblLine1 = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColorPalette.Text,
                BackColor = Color.Transparent,
            };
            _lblLine2 = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = ColorPalette.SubText,
                BackColor = Color.Transparent,
            };

            Controls.Add(_lblLine2);
            Controls.Add(_lblLine1);

            Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(ColorPalette.Border), 0, Height - 1, Width, Height - 1);
        }

        /// <summary>Cập nhật card với danh sách sinh viên của lớp vừa tìm.</summary>
        public void Update(string tenLop, List<SinhVienItem> items)
        {
            if (items == null || items.Count == 0)
            {
                Visible = false;
                return;
            }

            float dtb = items.Average(x =>
            {
                float.TryParse(x.Diem,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out float d);
                return d;
            });

            int xs  = items.Count(x => x.XepLoai == "Xuất sắc");
            int g   = items.Count(x => x.XepLoai == "Giỏi");
            int kha = items.Count(x => x.XepLoai == "Khá");
            int tb  = items.Count(x => x.XepLoai == "Trung bình");
            int yeu = items.Count(x => x.XepLoai == "Yếu");

            _lblLine1.Text =
                $"Lớp: {tenLop}   •   Tổng: {items.Count} SV   •   ĐTB lớp: {dtb:F2}";
            _lblLine2.Text =
                $"Xuất sắc: {xs}   Giỏi: {g}   Khá: {kha}   Trung bình: {tb}   Yếu: {yeu}";

            Visible = true;
        }

        public new void Hide() => Visible = false;
    }
}

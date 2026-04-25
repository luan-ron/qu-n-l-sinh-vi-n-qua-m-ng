using System.Drawing;

namespace ClientSinhVien.Helpers
{
    /// <summary>
    /// Tất cả màu sắc dark mode tập trung tại đây.
    /// Các class khác KHÔNG hardcode màu — luôn lấy từ đây.
    /// </summary>
    public static class ColorPalette
    {
        public static readonly Color BG       = Color.FromArgb( 18,  18,  35);
        public static readonly Color Sidebar  = Color.FromArgb( 25,  25,  50);
        public static readonly Color Card     = Color.FromArgb( 32,  32,  62);
        public static readonly Color Header   = Color.FromArgb( 22,  22,  48);
        public static readonly Color Accent   = Color.FromArgb( 99, 102, 241);
        public static readonly Color Accent2  = Color.FromArgb(139,  92, 246);
        public static readonly Color Success  = Color.FromArgb( 52, 211, 153);
        public static readonly Color Danger   = Color.FromArgb(248, 113, 113);
        public static readonly Color Warn     = Color.FromArgb(245, 158,  11);
        public static readonly Color Text     = Color.FromArgb(226, 232, 240);
        public static readonly Color SubText  = Color.FromArgb(148, 163, 184);
        public static readonly Color Border   = Color.FromArgb( 51,  51,  85);
        public static readonly Color RowAlt   = Color.FromArgb( 24,  24,  50);
        public static readonly Color RowSel   = Color.FromArgb( 55,  55, 110);
        public static readonly Color InputBg  = Color.FromArgb( 22,  22,  44);

        // Màu xếp loại (dùng cho Excel export và grid formatting)
        public static readonly Color GradeExcellent = Color.FromArgb( 52, 211, 153); // Xuất sắc
        public static readonly Color GradeGood      = Color.FromArgb( 99, 102, 241); // Giỏi
        public static readonly Color GradeFair      = Color.FromArgb(251, 191,  36); // Khá
        public static readonly Color GradeAverage   = Color.FromArgb(251, 146,  60); // Trung bình
        public static readonly Color GradePoor      = Color.FromArgb(248, 113, 113); // Yếu
    }
}

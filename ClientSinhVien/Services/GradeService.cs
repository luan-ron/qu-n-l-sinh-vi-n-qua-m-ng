using System.Drawing;
using ClientSinhVien.Helpers;

namespace ClientSinhVien.Services
{
    /// <summary>
    /// Tính xếp loại từ điểm số.
    /// Tất cả logic liên quan đến điểm/xếp loại tập trung tại đây.
    /// </summary>
    public static class GradeService
    {
        public static string GetGrade(float diem)
        {
            if (diem >= 9f)   return "Xuất sắc";
            if (diem >= 8f)   return "Giỏi";
            if (diem >= 6.5f) return "Khá";
            if (diem >= 5f)   return "Trung bình";
            return "Yếu";
        }

        public static Color GetGradeColor(string xepLoai)
        {
            switch (xepLoai)
            {
                case "Xuất sắc":  return ColorPalette.GradeExcellent;
                case "Giỏi":      return ColorPalette.GradeGood;
                case "Khá":       return ColorPalette.GradeFair;
                case "Trung bình":return ColorPalette.GradeAverage;
                case "Yếu":       return ColorPalette.GradePoor;
                default:          return ColorPalette.Text;
            }
        }

        /// <summary>Trả về hex color string cho EPPlus/ClosedXML (không có #)</summary>
        public static string GetGradeHex(string xepLoai)
        {
            switch (xepLoai)
            {
                case "Xuất sắc":  return "34D399";
                case "Giỏi":      return "6366F1";
                case "Khá":       return "FBBF24";
                case "Trung bình":return "FB923C";
                case "Yếu":       return "F87171";
                default:          return "FFFFFF";
            }
        }
    }
}

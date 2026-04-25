using System.Globalization;

namespace ServerSinhVien.Models
{
    /// <summary>
    /// Model sinh viên — serialization luôn dùng InvariantCulture
    /// để tránh lỗi dấu phẩy/chấm theo locale máy tính (vi-VN dùng dấu phẩy).
    /// </summary>
    public class SinhVien
    {
        public string MSSV  { get; set; }
        public string HoTen { get; set; }
        public string Lop   { get; set; }
        public float  Diem  { get; set; }

        // ── Serialize: dùng InvariantCulture → luôn ra "8.5" (dấu chấm) ──────
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3:F2}",
                MSSV, HoTen, Lop, Diem);
        }

        // ── Deserialize: chấp nhận cả "8.5" và "8,5" để tương thích dữ liệu cũ
        public static SinhVien FromString(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new System.FormatException("Dữ liệu rỗng");

            var parts = data.Split('|');
            if (parts.Length < 4)
                throw new System.FormatException("Thiếu trường dữ liệu: " + data);

            float diem = 0;
            if (!float.TryParse(parts[3].Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out diem))
                float.TryParse(parts[3].Trim(), NumberStyles.Any,
                    CultureInfo.CurrentCulture, out diem);

            return new SinhVien
            {
                MSSV  = parts[0].Trim(),
                HoTen = parts[1].Trim(),
                Lop   = parts[2].Trim(),
                Diem  = diem,
            };
        }
    }
}

namespace ClientSinhVien.Models
{
    /// <summary>
    /// DTO dùng để binding vào DataGridView.
    /// Không chứa business logic — chỉ là data holder.
    /// </summary>
    public class SinhVienItem
    {
        public string MSSV    { get; set; }
        public string HoTen   { get; set; }
        public string Lop     { get; set; }
        public string Diem    { get; set; }
        public string XepLoai { get; set; }
    }
}

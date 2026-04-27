using System;
using System.Collections.Generic;
using ClientSinhVien.Models;

namespace ClientSinhVien.Services
{
    /// <summary>
    /// Quản lý bộ nhớ đệm (cache) danh sách sinh viên trong RAM.
    /// Dùng để sort/filter nhanh mà không cần gọi lại server,
    /// và hiển thị dữ liệu khi mất kết nối.
    /// </summary>
    public class CacheService
    {
        private List<SinhVienItem> cache = new List<SinhVienItem>();
        private DateTime lastUpdate = DateTime.MinValue;

        public bool HasData => cache.Count > 0;
        public int  Count   => cache.Count;
        public DateTime LastUpdate => lastUpdate;

        // ── Cập nhật cache từ dữ liệu mới nhận ──────────────────────────────
        public void Update(List<SinhVienItem> items)
        {
            cache = new List<SinhVienItem>(items);
            lastUpdate = DateTime.Now;
        }

        // ── Lấy toàn bộ cache ────────────────────────────────────────────────
        public List<SinhVienItem> GetAll()
        {
            return new List<SinhVienItem>(cache);
        }

        // ── Xóa cache ────────────────────────────────────────────────────────
        public void Clear()
        {
            cache.Clear();
        }

        // ── Trả về chuỗi hiển thị trạng thái cache ───────────────────────────
        public string StatusText
        {
            get
            {
                if (cache.Count == 0) return "Cache: trống";
                return $"Cache: {cache.Count} sinh viên | Cập nhật lúc {lastUpdate:HH:mm:ss}";
            }
        }
    }
}

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
        private List<SinhVienItem> _cache     = new List<SinhVienItem>();
        private DateTime           _lastUpdate = DateTime.MinValue;

        public bool HasData => _cache.Count > 0;
        public int  Count   => _cache.Count;
        public DateTime LastUpdate => _lastUpdate;

        // ── Cập nhật cache từ dữ liệu mới nhận ──────────────────────────────
        public void Update(List<SinhVienItem> items)
        {
            _cache      = new List<SinhVienItem>(items);
            _lastUpdate = DateTime.Now;
        }

        // ── Lấy toàn bộ cache ────────────────────────────────────────────────
        public List<SinhVienItem> GetAll()
        {
            return new List<SinhVienItem>(_cache);
        }

        // ── Xóa cache ────────────────────────────────────────────────────────
        public void Clear()
        {
            _cache.Clear();
        }

        // ── Trả về chuỗi hiển thị trạng thái cache ───────────────────────────
        public string StatusText
        {
            get
            {
                if (_cache.Count == 0) return "Cache: trống";
                return $"Cache: {_cache.Count} sinh viên | Cập nhật lúc {_lastUpdate:HH:mm:ss}";
            }
        }
    }
}

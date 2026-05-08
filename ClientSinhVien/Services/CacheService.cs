using System;
using System.Collections.Generic;
using ClientSinhVien.Models;

namespace ClientSinhVien.Services
{

    public class CacheService
    {
        private List<SinhVienItem> cache = new List<SinhVienItem>();
        private DateTime lastUpdate = DateTime.MinValue;

        public bool HasData => cache.Count > 0;
        public int  Count   => cache.Count;
        public DateTime LastUpdate => lastUpdate;

        public void Update(List<SinhVienItem> items)
        {
            cache = new List<SinhVienItem>(items);
            lastUpdate = DateTime.Now;
        }

        public List<SinhVienItem> GetAll()
        {
            return new List<SinhVienItem>(cache);
        }

        public void Clear()
        {
            cache.Clear();
        }

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ServerSinhVien.Models;

namespace ServerSinhVien.Data
{
    /// <summary>
    /// Chịu trách nhiệm đọc/ghi file data.txt và quản lý List&lt;SinhVien&gt; trong RAM.
    /// </summary>
    public class SinhVienRepository
    {
        private readonly List<SinhVien> _ds   = new List<SinhVien>();
        private readonly object         _lock = new object();
        private readonly string         _file;

        public SinhVienRepository()
        {
            _file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.txt");
        }

        // ── Trả về lock object để Service dùng khi cần atomic operation ──────
        public object Lock => _lock;

        // ── Trả về bản sao danh sách (thread-safe) ────────────────────────────
        public List<SinhVien> GetAll()
        {
            lock (_lock)
                return new List<SinhVien>(_ds);
        }

        // ── Thêm sinh viên vào ────────────────────────────────────────────────────
        public void Add(SinhVien sv)
        {
            lock (_lock)
            {
                _ds.Add(sv);
                Save();
            }
        }

        // ── Cập nhật sinh viên (theo MSSV cũ) ────────────────────────────────
        public bool Update(string mssvCu, SinhVien svMoi)
        {
            lock (_lock)
            {
                var old = _ds.Find(s => s.MSSV == mssvCu);
                if (old == null) return false;
                old.MSSV  = svMoi.MSSV;
                old.HoTen = svMoi.HoTen;
                old.Lop   = svMoi.Lop;
                old.Diem  = svMoi.Diem;
                Save();
                return true;
            }
        }

        // ── Xóa sinh viên ────────────────────────────────────────────────────
        public int Delete(string mssv)
        {
            lock (_lock)
            {
                int removed = _ds.RemoveAll(s => s.MSSV == mssv);
                if (removed > 0) Save();
                return removed;
            }
        }

        // ── Kiểm tra MSSV đã tồn tại ─────────────────────────────────────────
        public bool Exists(string mssv)
        {
            lock (_lock)
                return _ds.Exists(s => s.MSSV == mssv);
        }

        // ── Tìm theo MSSV ─────────────────────────────────────────────────────
        public SinhVien FindByMSSV(string mssv)
        {
            lock (_lock)
                return _ds.Find(s => s.MSSV == mssv);
        }

        // ── Load từ file ──────────────────────────────────────────────────────
        public void Load()
        {
            if (!File.Exists(_file))
            {
                Log("Chưa có file data.txt — bắt đầu với danh sách rỗng.", ConsoleColor.DarkYellow);
                return;
            }
            int loaded = 0, skipped = 0;
            foreach (var line in File.ReadAllLines(_file, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try   { _ds.Add(SinhVien.FromString(line)); loaded++; }
                catch { skipped++; }
            }
            Log($"Đã load {loaded} sinh viên từ {_file}" +
                (skipped > 0 ? $" (bỏ qua {skipped} dòng lỗi)" : ""),
                ConsoleColor.Green);
        }

        // ── Ghi ra file ───────────────────────────────────────────────────────
        private void Save()
        {
            File.WriteAllLines(_file, _ds.ConvertAll(s => s.ToString()), Encoding.UTF8);
        }

        private static void Log(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
            Console.ResetColor();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using ClientSinhVien.Helpers;

namespace ClientSinhVien.Services
{
    public enum LogAction
    {
        Them,       // THÊM
        Xoa,        // XÓA
        CapNhat,    // CẬP NHẬT
        DoiMSSV     // ĐỔI MSSV
    }

    public class LogEntry
    {
        public DateTime  Time    { get; set; }
        public LogAction Action  { get; set; }
        public string    Message { get; set; }
        public Color     Color   { get; set; }
    }

    /// <summary>
    /// Quản lý lịch sử hoạt động trong RAM.
    /// Log chỉ tồn tại trong phiên chạy — không persist.
    /// </summary>
    public class LogService
    {
        private readonly List<LogEntry> _entries = new List<LogEntry>();

        public IReadOnlyList<LogEntry> Entries => _entries.AsReadOnly();

        // ── Thêm log ──────────────────────────────────────────────────────────
        public LogEntry Add(LogAction action, string detail)
        {
            string prefix;
            Color  color;

            switch (action)
            {
                case LogAction.Them:
                    prefix = "THÊM    ";
                    color  = ColorPalette.Success;
                    break;
                case LogAction.Xoa:
                    prefix = "XÓA     ";
                    color  = ColorPalette.Danger;
                    break;
                case LogAction.CapNhat:
                    prefix = "CẬP NHẬT";
                    color  = ColorPalette.Accent;
                    break;
                case LogAction.DoiMSSV:
                    prefix = "ĐỔI MSSV";
                    color  = ColorPalette.Warn;
                    break;
                default:
                    prefix = "INFO    ";
                    color  = ColorPalette.SubText;
                    break;
            }

            var entry = new LogEntry
            {
                Time    = DateTime.Now,
                Action  = action,
                Message = $"[{DateTime.Now:HH:mm:ss}]  {prefix}  {detail}",
                Color   = color
            };
            _entries.Add(entry);
            return entry;
        }

        // ── Xóa toàn bộ log ───────────────────────────────────────────────────
        public void Clear() => _entries.Clear();
    }
}

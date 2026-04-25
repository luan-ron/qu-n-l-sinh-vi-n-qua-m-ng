using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ClientSinhVien.Models;
using ClientSinhVien.Services;

namespace ClientSinhVien.Services
{
    /// <summary>
    /// Xuất danh sách sinh viên ra file Excel (.xlsx).
    /// Dùng EPPlus 4.x (không cần LicenseContext).
    /// NuGet: Install-Package EPPlus -Version 4.5.3.3
    /// </summary>
    public static class ExportService
    {
        public static void ExportToExcel(List<SinhVienItem> items, string filePath)
        {
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Danh Sách Sinh Viên");

                // ── Header row ───────────────────────────────────────────────
                string[] headers = { "STT", "MSSV", "Họ và Tên", "Lớp", "Điểm TB", "Xếp Loại" };
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = ws.Cells[1, col];
                    cell.Value = headers[col - 1];
                    cell.Style.Font.Bold      = true;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(99, 102, 241)); // C_ACCENT
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment   = ExcelVerticalAlignment.Center;
                }
                ws.Row(1).Height = 24;

                // ── Data rows ────────────────────────────────────────────────
                for (int i = 0; i < items.Count; i++)
                {
                    int row = i + 2;
                    var sv  = items[i];

                    ws.Cells[row, 1].Value = i + 1;
                    ws.Cells[row, 2].Value = sv.MSSV;
                    ws.Cells[row, 3].Value = sv.HoTen;
                    ws.Cells[row, 4].Value = sv.Lop;
                    ws.Cells[row, 5].Value = sv.Diem;
                    ws.Cells[row, 6].Value = sv.XepLoai;

                    // Tô màu cột Xếp Loại
                    string hex   = GradeService.GetGradeHex(sv.XepLoai);
                    var    grade = ws.Cells[row, 6];
                    grade.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    grade.Style.Fill.BackgroundColor.SetColor(HexToColor(hex));
                    grade.Style.Font.Bold = true;
                    grade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Căn giữa cột STT và Điểm TB
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Row(row).Height = 20;
                }

                // ── Auto fit columns ──────────────────────────────────────────
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                // Đặt min width cho cột Họ tên
                if (ws.Column(3).Width < 25) ws.Column(3).Width = 25;

                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16));
        }
    }
}

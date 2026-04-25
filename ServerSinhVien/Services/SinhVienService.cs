using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerSinhVien.Data;
using ServerSinhVien.Models;

namespace ServerSinhVien.Services
{
    /// <summary>
    /// File: SinhVienService.cs
    /// Chức năng: Business logic của Server, nhận các lệnh từ Client (như GET, ADD, UPDATE, SEARCH,...)
    /// và thực thi chúng trên dữ liệu thông qua SinhVienRepository, sau đó trả về chuỗi kết quả.
    /// Commands:
    ///   GET                              → lấy toàn bộ danh sách SV
    ///   GETSORTED;lop                    → lấy danh sách và sắp xếp theo Lớp
    ///   SEARCH;keyword                   → tìm theo MSSV/HoTen/Lop
    ///   SEARCHDIEM;min|max               → tìm SV có Điểm trong khoảng
    ///   SEARCHLOP;ten_lop                → tìm SV thuộc lớp
    ///   SEARCHMULTI;keyword|min|max|lop  → tìm kiếm kết hợp nhiều điều kiện cùng lúc
    ///   ADD;MSSV|HoTen|Lop|Diem         → thêm SV
    ///   UPDATE;MSSV|HoTen|Lop|Diem      → cập nhật SV
    ///   UPDATEMSSV;MSSV_CU|...          → đổi MSSV
    ///   DELETE;MSSV                      → xóa SV
    /// </summary>
    public class SinhVienService
    {
        private readonly SinhVienRepository _repo;

        public SinhVienService(SinhVienRepository repo)
        {
            _repo = repo;
        }

        public string Process(string req)
        {
            try
            {
                var parts = req.Split(new[] { ';' }, 2); // Tách chuỗi yêu cầu thành 2 phần: Lệnh và Tham số, cắt tại dấu chấm phẩy đầu tiên
                string cmd = parts[0].Trim().ToUpper(); // Lấy tên lệnh (phần 1), xóa khoảng trắng và viết hoa

                switch (cmd) // Kiểm tra lệnh thuộc loại nào
                {
                    case "GET": // Trường hợp lấy toàn bộ danh sách
                        return SerializeList(_repo.GetAll()); // Lấy dữ liệu từ repo, chuyển thành chuỗi và trả về

                    case "GETSORTED": // Trường hợp lấy danh sách và sắp xếp
                    {
                        string dir = parts.Length > 1 ? parts[1].Trim().ToLower() : "asc"; // Lấy hướng sắp xếp (asc hoặc desc), mặc định là asc
                        var list = _repo.GetAll(); // Lấy toàn bộ danh sách từ repo
                        if (dir == "desc") // Nếu yêu cầu sắp xếp giảm dần
                            list = list.OrderByDescending(s => s.Lop, StringComparer.OrdinalIgnoreCase).ToList(); // Sắp xếp theo cột Lớp giảm dần
                        else // Ngược lại
                            list = list.OrderBy(s => s.Lop, StringComparer.OrdinalIgnoreCase).ToList(); // Sắp xếp theo cột Lớp tăng dần
                        return SerializeList(list); // Chuyển danh sách đã sắp xếp thành chuỗi và trả về
                    }

                    case "SEARCH": // Trường hợp tìm kiếm cơ bản
                    {
                        string kw = (parts.Length > 1 ? parts[1] : "").Trim().ToLower(); // Lấy từ khóa tìm kiếm
                        var kq = _repo.GetAll().FindAll(s => // Tìm trong danh sách các sinh viên thỏa mãn điều kiện
                            s.MSSV.ToLower().Contains(kw)  || // MSSV chứa từ khóa
                            s.HoTen.ToLower().Contains(kw) || // Hoặc Tên chứa từ khóa
                            s.Lop.ToLower().Contains(kw)); // Hoặc Lớp chứa từ khóa
                        return SerializeList(kq); // Chuyển kết quả thành chuỗi
                    }

                    case "SEARCHDIEM": // Trường hợp tìm theo khoảng điểm
                    {
                        if (parts.Length < 2) return "ERROR;Thiếu tham số min|max"; // Kiểm tra có truyền tham số điểm không
                        var p2 = parts[1].Split('|'); // Tách tham số để lấy điểm min và max
                        if (p2.Length < 2) return "ERROR;Định dạng phải là min|max"; // Kiểm tra đúng định dạng không
                        if (!float.TryParse(p2[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float mn)) // Chuyển min thành số
                            return "ERROR;Giá trị min không hợp lệ"; // Báo lỗi nếu min không phải là số
                        if (!float.TryParse(p2[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float mx)) // Chuyển max thành số
                            return "ERROR;Giá trị max không hợp lệ"; // Báo lỗi nếu max không phải là số
                        var kq = _repo.GetAll().FindAll(s => s.Diem >= mn && s.Diem <= mx); // Lọc danh sách sinh viên có điểm nằm trong khoảng [min, max]
                        return SerializeList(kq); // Trả về kết quả
                    }

                    case "SEARCHLOP": // Trường hợp tìm theo tên lớp
                    {
                        string lop = (parts.Length > 1 ? parts[1] : "").Trim().ToLower(); // Lấy thông tin lớp
                        var kq = _repo.GetAll().FindAll(s => // Lọc danh sách sinh viên
                            s.Lop.ToLower().Contains(lop)); // Kiểm tra xem tên lớp của sinh viên có chứa chuỗi tìm kiếm không
                        return SerializeList(kq); // Trả về kết quả
                    }

                    case "SEARCHMULTI": // Trường hợp tìm kiếm kết hợp nhiều điều kiện
                    {
                        if (parts.Length < 2) return "ERROR;Thiếu tham số"; // Kiểm tra có truyền tham số không
                        var p = parts[1].Split('|'); // Tách các tham số (từ khóa | min | max | lớp)
                        string kw = p[0].Trim().ToLower(); // Tham số đầu tiên là từ khóa
                        
                        float mn = 0f, mx = 10f; // Điểm min max mặc định
                        if (p.Length > 2) // Nếu có truyền tham số điểm
                        {
                            float.TryParse(p[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out mn); // Gán điểm min
                            float.TryParse(p[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out mx); // Gán điểm max
                        }
                        string lop = p.Length > 3 ? p[3].Trim().ToLower() : ""; // Nếu có truyền thông tin lớp thì lấy thông tin lớp

                        var kq = _repo.GetAll().FindAll(s => // Lọc sinh viên với điều kiện AND
                            (string.IsNullOrEmpty(kw) || s.MSSV.ToLower().Contains(kw) || s.HoTen.ToLower().Contains(kw)) && // Khớp từ khóa (nếu có)
                            (string.IsNullOrEmpty(lop) || s.Lop.ToLower().Contains(lop)) && // Khớp lớp (nếu có)
                            (s.Diem >= mn && s.Diem <= mx)); // Nằm trong khoảng điểm
                        return SerializeList(kq); // Trả về chuỗi kết quả
                    }

                    case "ADD": // Trường hợp thêm sinh viên mới
                    {
                        if (parts.Length < 2) return "ERROR;Thiếu dữ liệu sinh viên"; // Kiểm tra có truyền dữ liệu sinh viên không
                        var sv = SinhVien.FromString(parts[1]); // Phân tích chuỗi dữ liệu thành đối tượng SinhVien
                        if (_repo.Exists(sv.MSSV)) return "ERROR;MSSV đã tồn tại"; // Kiểm tra mã số sinh viên đã tồn tại trong database/list chưa
                        _repo.Add(sv); // Thêm sinh viên vào repo
                        Log($"  [ADD] {sv}", ConsoleColor.Green); // Ghi log thêm thành công
                        return "OK"; // Trả về thông báo thành công
                    }

                    case "UPDATE": // Trường hợp cập nhật sinh viên
                    {
                        if (parts.Length < 2) return "ERROR;Thiếu dữ liệu sinh viên"; // Kiểm tra có dữ liệu truyền vào không
                        var sv = SinhVien.FromString(parts[1]); // Phân tích chuỗi thành đối tượng
                        if (!_repo.Update(sv.MSSV, sv)) return "ERROR;Không tìm thấy MSSV"; // Cập nhật và báo lỗi nếu không tìm thấy sinh viên
                        Log($"  [UPD] {sv}", ConsoleColor.Yellow); // Ghi log cập nhật thành công
                        return "OK"; // Trả về thông báo thành công
                    }

                    case "UPDATEMSSV": // Trường hợp đổi MSSV
                    {
                        if (parts.Length < 2) return "ERROR;Thiếu dữ liệu"; // Kiểm tra xem có dữ liệu không
                        var fields = parts[1].Split('|'); // Tách các trường bằng dấu pipe (|)
                        if (fields.Length < 5) return "ERROR;Định dạng: MSSV_CU|MSSV_MOI|HoTen|Lop|Diem"; // Báo lỗi nếu thiếu số lượng trường cần thiết

                        string mssvCu  = fields[0].Trim(); // Lấy mã số sinh viên cũ
                        string mssvMoi = fields[1].Trim(); // Lấy mã số sinh viên mới

                        // Kiểm tra SV cũ tồn tại
                        if (_repo.FindByMSSV(mssvCu) == null) // Tìm trong repo xem có tồn tại mã cũ không
                            return "ERROR;Không tìm thấy MSSV cũ: " + mssvCu; // Báo lỗi không tìm thấy

                        // Kiểm tra MSSV mới không trùng với SV khác
                        if (mssvMoi != mssvCu && _repo.Exists(mssvMoi)) // Nếu đổi sang mã mới mà mã mới đã có người dùng
                            return "ERROR;MSSV mới đã tồn tại: " + mssvMoi; // Báo lỗi trùng MSSV

                        float diem = 0;
                        if (!float.TryParse(fields[4].Trim(), NumberStyles.Any,
                                CultureInfo.InvariantCulture, out diem)) // Ép kiểu điểm (dấu chấm)
                            float.TryParse(fields[4].Trim(), NumberStyles.Any,
                                CultureInfo.CurrentCulture, out diem); // Cố gắng ép kiểu theo định dạng hệ thống (dấu phẩy)

                        var svMoi = new SinhVien // Tạo đối tượng sinh viên mới với MSSV mới
                        {
                            MSSV  = mssvMoi, // Mã mới
                            HoTen = fields[2].Trim(), // Cập nhật tên
                            Lop   = fields[3].Trim(), // Cập nhật lớp
                            Diem  = diem // Cập nhật điểm
                        };
                        _repo.Update(mssvCu, svMoi); // Thực hiện thay thế sinh viên cũ bằng sinh viên mới trong repo
                        Log($"  [UPDMSSV] {mssvCu} → {mssvMoi}", ConsoleColor.Magenta); // Ghi log
                        return "OK"; // Trả về thành công
                    }

                    case "DELETE": // Trường hợp xóa sinh viên
                    {
                        if (parts.Length < 2) return "ERROR;Thiếu MSSV"; // Kiểm tra xem có mã cần xóa không
                        string mssv   = parts[1].Trim(); // Lấy mã
                        int    removed = _repo.Delete(mssv); // Gọi repo xóa và lấy số lượng bị xóa
                        if (removed == 0) return "ERROR;Không tìm thấy MSSV"; // Báo lỗi nếu xóa không thành công (không tìm thấy)
                        Log($"  [DEL] MSSV={mssv}", ConsoleColor.Red); // Ghi log xóa
                        return "OK"; // Trả về thành công
                    }

                    default: // Các trường hợp còn lại (lệnh lạ)
                        return "ERROR;Lệnh không hợp lệ: " + cmd; // Báo lỗi lệnh không hợp lệ
                }
            }
            catch (Exception ex)
            {
                return "ERROR;" + ex.Message; // Bắt các lỗi ngoại lệ không lường trước và trả về cho client
            }
        }

        // ── Serialize list thành chuỗi pipe-separated lines ─────────────────
        private static string SerializeList(List<SinhVien> list)
        {
            return list.Count > 0 ? string.Join("\n", list) : "\n";
        }

        private static void Log(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
            Console.ResetColor();
        }
    }
}

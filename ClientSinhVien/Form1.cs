using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClientSinhVien.Controls;
using ClientSinhVien.Helpers;
using ClientSinhVien.Models;
using ClientSinhVien.Network;
using ClientSinhVien.Panels;
using ClientSinhVien.Services;

namespace ClientSinhVien
{
    /// <summary>
    /// File: Form1.cs
    /// Chức năng: Giao diện chính của ứng dụng quản lý sinh viên.
    /// Quản lý sự kiện, gọi đến các Service, gửi lệnh thông qua Network,
    /// và cập nhật giao diện hiển thị cho người dùng.
    /// Tính năng:
    /// - Kết nối/Ngắt kết nối tới Server.
    /// - Thêm, Sửa, Xóa thông tin sinh viên (tương tác trực tiếp qua danh sách).
    /// - Tìm kiếm kết hợp (theo tên, mã, điểm, lớp).
    /// - Xuất file Excel.
    /// - Quản lý log các hoạt động.
    /// </summary>
    public partial class Form1 : Form
    {
        // ── Services ──────────────────────────────────────────────────────────
        private readonly ServerConnection _conn    = new ServerConnection();
        private readonly CacheService     _cache   = new CacheService();
        private readonly LogService       _log     = new LogService();

        // ── Panels & Controls ─────────────────────────────────────────────────
        private SidebarPanel  _sidebar;
        private TopBarPanel   _topBar;
        private LogPanel      _logPanel;
        private StatCard      _statCard;
        private DataGridView  _grid;
        private ToastStrip    _toast;
        private Label         _lblCacheStatus;
        private Panel         _centerPanel;
        private string        _selectedMSSV = null;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            WireEvents();
        }

        // ══════════════════════════════════════════════════════════════════════
        // BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            Text            = "Quản Lý Sinh Viên qua Mạng";
            Size            = new Size(1180, 720);
            MinimumSize     = new Size(900, 600);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = ColorPalette.BG;
            Font            = new Font("Segoe UI", 9f);

            // ── Toast strip (top, docked) ─────────────────────────────────
            _toast = new ToastStrip
            {
                Dock      = DockStyle.Top,
                Height    = 0,
                BackColor = ColorPalette.Success,
            };
            Controls.Add(_toast);

            // ── Sidebar ───────────────────────────────────────────────────
            _sidebar = new SidebarPanel();
            Controls.Add(_sidebar);

            // ── Center panel (top bar + stat card + grid + log panel) ─────
            _centerPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColorPalette.BG };
            Controls.Add(_centerPanel);
            _centerPanel.BringToFront();

            // ── Top bar ───────────────────────────────────────────────────
            _topBar = new TopBarPanel();
            _centerPanel.Controls.Add(_topBar);

            // Thứ tự add control vào _centerPanel phải đúng để DockStyle.Fill hoạt động:
            // Fill được tính CUỐI CÙNG, nên phải add _grid SAU tất cả Docked controls.
            // Trong WinForms: control add trước = Z-order cao hơn (hiển thị trên)
            // nhưng Dock layout xử lý theo thứ tự ngược lại (LIFO).
            // Vì vậy: add Bottom/Top controls TRƯỚC, add Fill CUỐI.

            // ── Bottom status bar ─────────────────────────────────────────
            var bottomBar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 24,
                BackColor = ColorPalette.Header,
            };
            _lblCacheStatus = new Label
            {
                Text      = "Cache: trống",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = ColorPalette.SubText,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent,
            };
            bottomBar.Controls.Add(_lblCacheStatus);
            _centerPanel.Controls.Add(bottomBar);

            // ── Log panel (Bottom) ────────────────────────────────────────
            _logPanel = new LogPanel();
            _centerPanel.Controls.Add(_logPanel);

            // ── Stat card (Top, hiện dưới TopBar khi tìm theo lớp) ───────
            _statCard = new StatCard();
            _centerPanel.Controls.Add(_statCard);

            // ── DataGridView (Fill — PHẢI add SAU tất cả Docked controls) ─
            _grid = BuildGrid();
            _centerPanel.Controls.Add(_grid);
            _grid.BringToFront();
        }

        private DataGridView BuildGrid()
        {
            var dg = new DataGridView
            {
                Dock                        = DockStyle.Fill,
                BackgroundColor             = ColorPalette.BG,
                GridColor                   = ColorPalette.Border,
                BorderStyle                 = BorderStyle.None,
                RowHeadersVisible           = false,
                ColumnHeadersVisible        = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight         = 38,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                ReadOnly                    = true,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate                 = { Height = 34 },
                Font                        = new Font("Segoe UI", 9.5f),
                EnableHeadersVisualStyles   = false,
                CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
            };

            // Header style — nền đậm, chữ trắng, bold, căn giữa
            dg.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = Color.FromArgb(30, 34, 54),
                ForeColor          = Color.FromArgb(200, 210, 255),
                Font               = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                SelectionBackColor = Color.FromArgb(30, 34, 54),
                SelectionForeColor = Color.FromArgb(200, 210, 255),
                Alignment          = DataGridViewContentAlignment.MiddleCenter,
                Padding            = new Padding(0, 0, 0, 0),
            };

            // Row style
            dg.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = ColorPalette.BG,
                ForeColor          = ColorPalette.Text,
                SelectionBackColor = ColorPalette.RowSel,
                SelectionForeColor = ColorPalette.Text,
                Padding            = new Padding(4, 0, 4, 0),
            };
            dg.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = ColorPalette.RowAlt,
                ForeColor          = ColorPalette.Text,
                SelectionBackColor = ColorPalette.RowSel,
                SelectionForeColor = ColorPalette.Text,
                Padding            = new Padding(4, 0, 4, 0),
            };

            // Columns — FillWeight tổng = 100
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MSSV", HeaderText = "MSSV",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HoTen", HeaderText = "Họ và Tên",
                FillWeight = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Lop", HeaderText = "Lớp",
                FillWeight = 18,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Diem", HeaderText = "Điểm TB",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "XepLoai", HeaderText = "Xếp Loại",
                FillWeight = 17,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });

            // Tô màu xếp loại
            dg.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dg.Columns["XepLoai"].Index && e.Value != null)
                {
                    e.CellStyle.ForeColor = GradeService.GetGradeColor(e.Value.ToString());
                    e.CellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            };

            // Click chọn hàng → điền vào form
            dg.SelectionChanged += (s, e) =>
            {
                if (dg.SelectedRows.Count == 0) return;
                var row = dg.SelectedRows[0];
                _selectedMSSV = row.Cells["MSSV"].Value?.ToString() ?? "";
                _sidebar.Fill(
                    _selectedMSSV,
                    row.Cells["HoTen"].Value?.ToString() ?? "",
                    row.Cells["Lop"].Value?.ToString()   ?? "",
                    row.Cells["Diem"].Value?.ToString()  ?? "");
            };

            return dg;
        }

        // ══════════════════════════════════════════════════════════════════════
        // WIRE EVENTS
        // ══════════════════════════════════════════════════════════════════════
        private void WireEvents()
        {
            _sidebar.BtnConnect.Click += OnConnect;
            _sidebar.BtnThem.Click    += OnThem;
            _sidebar.BtnCapNhat.Click += OnCapNhat;
            _sidebar.BtnXoa.Click     += OnXoa;
            _sidebar.BtnXoaForm.Click += (s, e) => {
                _sidebar.Clear();
                _selectedMSSV = null;
                if (_grid.SelectedRows.Count > 0) _grid.ClearSelection();
            };

            _topBar.BtnLoad.Click  += (s, e) => _ = LoadDataAsync();
            _topBar.BtnSort.Click  += OnSort;
            _topBar.BtnExcel.Click += OnExportExcel;

            _topBar.TxtSearch.KeyDown     += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; _ = ApplyFiltersAsync(); } };
            _topBar.TxtSearchDiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; _ = ApplyFiltersAsync(); } };
            _topBar.TxtSearchLop.KeyDown  += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; _ = ApplyFiltersAsync(); } };
        }

        // ══════════════════════════════════════════════════════════════════════
        // KẾT NỐI
        // ══════════════════════════════════════════════════════════════════════
        private async void OnConnect(object sender, EventArgs e)
        {
            if (_conn.IsConnected) // Kiểm tra xem nếu đã kết nối với server
            {
                _conn.Disconnect(); // Thực hiện ngắt kết nối
                _sidebar.SetConnected(false); // Cập nhật trạng thái giao diện ở thanh bên thành chưa kết nối
                ShowToast("Đã ngắt kết nối.", ColorPalette.Warn); // Hiển thị thông báo ngắt kết nối thành công
                return; // Kết thúc hàm
            }
            try
            {
                ShowToast("Đang kết nối...", ColorPalette.Accent); // Hiển thị thông báo đang tiến hành kết nối
                await _conn.ConnectAsync(_sidebar.TxtHost.Text.Trim(), int.Parse(_sidebar.TxtPort.Text.Trim())); // Gọi hàm kết nối tới địa chỉ IP và Port nhập trên giao diện
                _sidebar.SetConnected(true); // Cập nhật trạng thái giao diện thành đã kết nối
                ShowToast("Kết nối thành công!", ColorPalette.Success); // Hiển thị thông báo kết nối thành công
                await LoadDataAsync(); // Tự động tải danh sách sinh viên ngay sau khi kết nối
            }
            catch (Exception ex)
            {
                _sidebar.SetConnected(false); // Đảm bảo trạng thái giao diện là chưa kết nối nếu có lỗi
                ShowToast("Lỗi kết nối: " + ex.Message, ColorPalette.Danger); // Hiển thị thông báo lỗi chi tiết
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // LOAD / SORT
        // ══════════════════════════════════════════════════════════════════════
        private async Task LoadDataAsync()
        {
            try
            {
                string resp = await _conn.SendAsync("GET\n"); // Gửi lệnh "GET\n" tới server để lấy toàn bộ danh sách sinh viên
                var items   = ParseResponse(resp); // Phân tích chuỗi dữ liệu phản hồi từ server thành danh sách đối tượng SinhVienItem
                _cache.Update(items); // Cập nhật dữ liệu mới tải về vào bộ nhớ tạm (Cache) để dùng khi offline
                BindGrid(items); // Hiển thị danh sách sinh viên lên bảng DataGridView
                _statCard.Hide(); // Ẩn bảng thống kê số lượng (chỉ hiển thị khi tìm kiếm theo lớp)
                UpdateCacheLabel(); // Cập nhật lại nhãn trạng thái hiển thị số lượng bộ nhớ đệm
            }
            catch
            {
                if (_cache.HasData) // Nếu có lỗi mạng nhưng trong bộ nhớ đệm vẫn có dữ liệu
                {
                    BindGrid(_cache.GetAll()); // Hiển thị danh sách từ bộ nhớ đệm lên giao diện
                    ShowToast("Mất kết nối — đang hiển thị dữ liệu cache.", ColorPalette.Warn); // Thông báo cho người dùng biết đang dùng dữ liệu offline
                }
                else ShowToast("Không thể tải dữ liệu.", ColorPalette.Danger); // Nếu cache cũng rỗng thì báo lỗi không tải được dữ liệu
            }
        }

        private async void OnSort(object sender, EventArgs e)
        {
            try
            {
                string dir  = _topBar.SortAscending ? "asc" : "desc"; // Xác định hướng sắp xếp: asc (tăng dần) hoặc desc (giảm dần) dựa vào nút bấm
                string resp = await _conn.SendAsync($"GETSORTED;{dir}\n"); // Gửi lệnh "GETSORTED;hướng" lên server để yêu cầu server trả về dữ liệu đã sắp xếp
                var items   = ParseResponse(resp); // Phân tích chuỗi trả về thành danh sách sinh viên
                _cache.Update(items); // Cập nhật dữ liệu đã sắp xếp vào bộ nhớ đệm
                BindGrid(items); // Hiển thị dữ liệu lên bảng
                UpdateCacheLabel(); // Cập nhật trạng thái bộ nhớ đệm
                ShowToast($"Đã sắp xếp theo Lớp {(dir == "asc" ? "A→Z" : "Z→A")}.", ColorPalette.Accent); // Thông báo thành công
            }
            catch (Exception ex)
            {
                // Fallback: sort từ cache nếu rớt mạng
                var sorted = _cache.GetAll(); // Lấy dữ liệu hiện tại từ bộ nhớ đệm
                sorted = _topBar.SortAscending // Nếu đang muốn sắp xếp tăng dần
                    ? sorted.OrderBy(x => x.Lop, StringComparer.OrdinalIgnoreCase).ToList() // Sắp xếp list offline theo Lớp tăng dần
                    : sorted.OrderByDescending(x => x.Lop, StringComparer.OrdinalIgnoreCase).ToList(); // Sắp xếp list offline theo Lớp giảm dần
                BindGrid(sorted); // Cập nhật lại giao diện bảng với dữ liệu đã sắp xếp offline
                ShowToast("Sort từ cache (offline): " + ex.Message, ColorPalette.Warn); // Thông báo đang sử dụng chế độ offline
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // THÊM / CẬP NHẬT / XÓA
        // ══════════════════════════════════════════════════════════════════════
        private async void OnThem(object sender, EventArgs e)
        {
            if (!ValidateFields(out string mssv, out string hoTen, out string lop, out float diem)) return; // Kiểm tra tính hợp lệ của dữ liệu đầu vào. Nếu sai thì dừng hàm.
            try
            {
                string cmd  = $"ADD;{mssv}|{hoTen}|{lop}|{diem.ToString(CultureInfo.InvariantCulture)}\n"; // Tạo lệnh "ADD;Mã|Tên|Lớp|Điểm"
                string resp = await _conn.SendAsync(cmd); // Gửi lệnh yêu cầu thêm sinh viên lên server và chờ phản hồi
                if (resp.StartsWith("OK")) // Nếu server trả về bắt đầu bằng "OK" nghĩa là thêm thành công
                {
                    var entry = _log.Add(LogAction.Them, $"{mssv} — {hoTen} — Lớp {lop} — Điểm {diem}"); // Ghi lại lịch sử hành động thêm vào hệ thống log
                    _logPanel.AppendEntry(entry); // Hiển thị dòng log lên giao diện bảng log
                    ShowToast("Thêm sinh viên thành công!", ColorPalette.Success); // Thông báo thêm thành công
                    _sidebar.Clear(); // Xóa trống các ô nhập liệu bên thanh công cụ
                    _selectedMSSV = null; // Bỏ chọn sinh viên hiện tại
                    if (_grid.SelectedRows.Count > 0) _grid.ClearSelection(); // Bỏ bôi đen dòng trên bảng DataGridView
                    await LoadDataAsync(); // Tải lại toàn bộ dữ liệu mới từ server về
                }
                else ShowToast(resp.Replace("ERROR;", ""), ColorPalette.Danger); // Nếu có lỗi (ERROR), hiển thị lỗi do server trả về
            }
            catch (Exception ex) { ShowToast("Lỗi: " + ex.Message, ColorPalette.Danger); } // Hiển thị lỗi ngoại lệ nếu không gửi được
        }

        private async void OnCapNhat(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedMSSV)) // Kiểm tra xem người dùng đã chọn sinh viên trên bảng chưa
            {
                ShowToast("Vui lòng chọn sinh viên từ danh sách để cập nhật.", ColorPalette.Warn); // Cảnh báo nếu chưa chọn
                return; // Dừng hàm
            }

            if (!ValidateFields(out string mssv, out string hoTen, out string lop, out float diem)) return; // Kiểm tra các thông tin nhập liệu có đúng định dạng không
            
            if (mssv != _selectedMSSV) // Kiểm tra nếu người dùng cố ý sửa mã sinh viên (MSSV)
            {
                ShowToast("Không được phép thay đổi ID. Chỉ được đổi tên, lớp, điểm.", ColorPalette.Danger); // Cảnh báo không cho phép đổi MSSV
                _sidebar.TxtMSSV.Text = _selectedMSSV; // Trả lại mã MSSV ban đầu vào ô textbox
                return; // Dừng hàm
            }

            try
            {
                string cmd = $"UPDATE;{mssv}|{hoTen}|{lop}|{diem.ToString(CultureInfo.InvariantCulture)}\n"; // Tạo lệnh "UPDATE;Mã|Tên|Lớp|Điểm"
                string resp = await _conn.SendAsync(cmd); // Gửi lệnh cập nhật lên server
                if (resp.StartsWith("OK")) // Nếu server cập nhật thành công và trả về OK
                {
                    var entry = _log.Add(LogAction.CapNhat, $"{mssv} — {hoTen} — Lớp {lop} — Điểm {diem}"); // Ghi log hành động cập nhật
                    _logPanel.AppendEntry(entry); // Hiện log lên màn hình
                    ShowToast("Cập nhật thành công!", ColorPalette.Success); // Thông báo thành công
                }
                else { ShowToast(resp.Replace("ERROR;", ""), ColorPalette.Danger); return; } // Nếu thất bại, thông báo lỗi từ server và dừng hàm
                _sidebar.Clear(); // Xóa sạch các trường nhập liệu
                _selectedMSSV = null; // Bỏ chọn sinh viên hiện tại
                if (_grid.SelectedRows.Count > 0) _grid.ClearSelection(); // Bỏ chọn dòng trên bảng
                await LoadDataAsync(); // Tải lại danh sách từ server để làm mới bảng
            }
            catch (Exception ex) { ShowToast("Lỗi: " + ex.Message, ColorPalette.Danger); } // Thông báo nếu bị lỗi mạng
        }

        private async void OnXoa(object sender, EventArgs e)
        {
            string mssv = _sidebar.TxtMSSV.Text.Trim(); // Lấy mã số sinh viên từ ô nhập liệu
            if (string.IsNullOrEmpty(mssv)) { ShowToast("Chưa chọn sinh viên cần xóa.", ColorPalette.Warn); return; } // Báo lỗi nếu mã trống

            if (MessageBox.Show($"Xóa sinh viên {mssv}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return; // Hiển thị hộp thoại xác nhận. Nếu chọn No thì hủy bỏ.
            try
            {
                string resp = await _conn.SendAsync($"DELETE;{mssv}\n"); // Gửi lệnh "DELETE;Mã" lên server
                if (resp.StartsWith("OK")) // Nếu server xóa thành công
                {
                    var entry = _log.Add(LogAction.Xoa, $"{mssv} — {_sidebar.TxtHoTen.Text.Trim()}"); // Ghi log thao tác xóa
                    _logPanel.AppendEntry(entry); // Cập nhật log lên giao diện
                    ShowToast("Xóa thành công!", ColorPalette.Success); // Báo thành công
                    _sidebar.Clear(); // Xóa form nhập liệu
                    _selectedMSSV = null; // Xóa trạng thái đang chọn
                    if (_grid.SelectedRows.Count > 0) _grid.ClearSelection(); // Xóa bôi đen trên bảng
                    await LoadDataAsync(); // Tải lại toàn bộ dữ liệu mới nhất
                }
                else ShowToast(resp.Replace("ERROR;", ""), ColorPalette.Danger); // Nếu có lỗi từ server, in ra
            }
            catch (Exception ex) { ShowToast("Lỗi: " + ex.Message, ColorPalette.Danger); } // In lỗi khi lỗi mạng
        }

        // ══════════════════════════════════════════════════════════════════════
        // TÌM KIẾM (Combined Filters)
        // ══════════════════════════════════════════════════════════════════════
        private async Task ApplyFiltersAsync()
        {
            string kw = _topBar.TxtSearch.Text.Trim(); // Lấy từ khóa tìm kiếm (tên hoặc MSSV)
            string lop = _topBar.TxtSearchLop.Text.Trim(); // Lấy thông tin lớp để lọc
            string diemInput = _topBar.TxtSearchDiem.Text.Trim(); // Lấy khoảng điểm hoặc điểm chính xác để lọc

            float min = 0f, max = 10f; // Mặc định điểm min là 0, max là 10
            if (!string.IsNullOrEmpty(diemInput)) // Nếu người dùng có nhập điểm cần lọc
            {
                diemInput = diemInput.Replace(" ", ""); // Xóa các dấu cách thừa
                try
                {
                    if (diemInput.Contains("-")) // Nếu định dạng là từ A-B (VD: 5-8)
                    {
                        var p = diemInput.Split('-'); // Cắt chuỗi bởi dấu trừ
                        min = float.Parse(p[0], CultureInfo.InvariantCulture); // Lấy số thứ nhất làm điểm tối thiểu
                        max = float.Parse(p[1], CultureInfo.InvariantCulture); // Lấy số thứ hai làm điểm tối đa
                    }
                    else if (diemInput.StartsWith(">=")) // Nếu có dấu >= (VD: >=5)
                        min = float.Parse(diemInput.Substring(2), CultureInfo.InvariantCulture); // Lấy phần số làm min
                    else if (diemInput.StartsWith("<=")) // Nếu có dấu <=
                        max = float.Parse(diemInput.Substring(2), CultureInfo.InvariantCulture); // Lấy phần số làm max
                    else if (diemInput.StartsWith(">")) // Nếu có dấu >
                        min = float.Parse(diemInput.Substring(1), CultureInfo.InvariantCulture) + 0.01f; // Tăng min lên 0.01 để loại trừ điểm bằng
                    else if (diemInput.StartsWith("<")) // Nếu có dấu <
                        max = float.Parse(diemInput.Substring(1), CultureInfo.InvariantCulture) - 0.01f; // Giảm max xuống 0.01
                    else
                        min = max = float.Parse(diemInput, CultureInfo.InvariantCulture); // Tìm chính xác một mức điểm
                }
                catch
                {
                    ShowToast("Định dạng điểm không hợp lệ. Vd: 7.0-9.0", ColorPalette.Warn); // Báo lỗi nếu parse số thất bại
                    return; // Thoát hàm nếu nhập điểm sai định dạng
                }
            }

            try
            {
                string cmd = $"SEARCHMULTI;{kw}|{min.ToString(CultureInfo.InvariantCulture)}|{max.ToString(CultureInfo.InvariantCulture)}|{lop}\n"; // Ghép lệnh "SEARCHMULTI;từ_khóa|min|max|lớp"
                string resp = await _conn.SendAsync(cmd); // Gửi lệnh tìm kiếm lên server
                var items = ParseResponse(resp); // Phân tích chuỗi trả về thành danh sách sinh viên khớp điều kiện
                _cache.Update(items); // Cập nhật danh sách này vào cache
                BindGrid(items); // Gắn danh sách lên DataGridView
                if (!string.IsNullOrEmpty(lop)) _statCard.Update(lop, items); // Nếu có tìm theo lớp, hiển thị thống kê học lực của lớp đó
                else _statCard.Hide(); // Nếu không, ẩn thanh thống kê đi
                UpdateCacheLabel(); // Cập nhật nhãn thống kê bộ nhớ đệm
                ShowToast($"Đã tìm thấy {items.Count} sinh viên thỏa mãn điều kiện.", ColorPalette.Accent); // Thông báo số lượng tìm được
            }
            catch (Exception ex)
            {
                ShowToast("Lỗi tìm kiếm: " + ex.Message, ColorPalette.Danger); // Thông báo lỗi nếu thất bại
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // XUẤT EXCEL
        // ══════════════════════════════════════════════════════════════════════
        private void OnExportExcel(object sender, EventArgs e)
        {
            var items = _cache.GetAll();
            if (items.Count == 0) { ShowToast("Không có dữ liệu để xuất.", ColorPalette.Warn); return; }

            using (var dlg = new SaveFileDialog
            {
                Title      = "Xuất danh sách sinh viên",
                Filter     = "Excel files (*.xlsx)|*.xlsx",
                FileName   = $"SinhVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    ExportService.ExportToExcel(items, dlg.FileName);
                    ShowToast($"Xuất Excel thành công: {dlg.FileName}", ColorPalette.Success);
                }
                catch (Exception ex)
                {
                    ShowToast("Lỗi xuất Excel: " + ex.Message, ColorPalette.Danger);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Parse response từ server thành List&lt;SinhVienItem&gt;.</summary>
        private List<SinhVienItem> ParseResponse(string resp)
        {
            var result = new List<SinhVienItem>();
            if (string.IsNullOrWhiteSpace(resp)) return result;
            foreach (var line in resp.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = line.Trim().Split('|');
                if (p.Length < 4) continue;
                float.TryParse(p[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float d);
                result.Add(new SinhVienItem
                {
                    MSSV    = p[0].Trim(),
                    HoTen   = p[1].Trim(),
                    Lop     = p[2].Trim(),
                    Diem    = d.ToString("F2", CultureInfo.InvariantCulture),
                    XepLoai = GradeService.GetGrade(d),
                });
            }
            return result;
        }

        /// <summary>Bind danh sách vào DataGridView (thread-safe).</summary>
        private void BindGrid(List<SinhVienItem> items)
        {
            if (InvokeRequired) { Invoke((Action)(() => BindGrid(items))); return; }
            _grid.Rows.Clear();
            foreach (var sv in items)
                _grid.Rows.Add(sv.MSSV, sv.HoTen, sv.Lop, sv.Diem, sv.XepLoai);
        }

        /// <summary>Validate fields nhập liệu.</summary>
        private bool ValidateFields(out string mssv, out string hoTen, out string lop, out float diem)
        {
            mssv  = _sidebar.TxtMSSV.Text.Trim();
            hoTen = _sidebar.TxtHoTen.Text.Trim();
            lop   = _sidebar.TxtLop.Text.Trim();
            diem  = 0;

            if (string.IsNullOrEmpty(mssv))  { ShowToast("Vui lòng nhập MSSV.",     ColorPalette.Warn); return false; }
            if (string.IsNullOrEmpty(hoTen)) { ShowToast("Vui lòng nhập Họ tên.",   ColorPalette.Warn); return false; }
            if (string.IsNullOrEmpty(lop))   { ShowToast("Vui lòng nhập Lớp.",      ColorPalette.Warn); return false; }
            if (!float.TryParse(_sidebar.TxtDiem.Text.Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out diem) || diem < 0 || diem > 10)
            {
                ShowToast("Điểm không hợp lệ (0 – 10).", ColorPalette.Warn); return false;
            }
            return true;
        }

        /// <summary>Hiển thị toast bar tự ẩn sau 3 giây.</summary>
        private void ShowToast(string msg, Color color)
        {
            if (InvokeRequired) { Invoke((Action)(() => ShowToast(msg, color))); return; }
            _toast.Msg       = msg;
            _toast.BackColor = color;
            _toast.Height    = 28;
            _toast.Invalidate();

            var timer = new Timer { Interval = 3000 };
            timer.Tick += (s, e) => { _toast.Height = 0; timer.Stop(); timer.Dispose(); };
            timer.Start();
        }

        private void UpdateCacheLabel()
        {
            if (InvokeRequired) { Invoke((Action)UpdateCacheLabel); return; }
            _lblCacheStatus.Text = _cache.StatusText;
        }
    }
}

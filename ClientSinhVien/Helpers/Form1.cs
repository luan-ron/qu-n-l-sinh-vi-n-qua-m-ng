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
    public partial class Form1 : Form
    {
        private readonly ServerConnection _conn = new ServerConnection();
        private readonly CacheService _cache = new CacheService();
        private readonly LogService _log = new LogService();

        private SidebarPanel _sidebar;
        private TopBarPanel _topBar;
        private LogPanel _logPanel;
        private StatCard _statCard;
        private DataGridView _grid;
        private ToastStrip _toast;
        private Label _lblCacheStatus;
        private Panel _centerPanel;
        private string _selectedMSSV = null;
        public Form1()
        {
            InitializeComponent();
            BuildUI();
            WireEvents();
        }
        private void BuildUI()
        {
            Text = "Quản Lý Sinh Viên qua Mạng";
            Size = new Size(1180, 720);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorPalette.BG;
            Font = new Font("Segoe UI", 9f);

            _toast = new ToastStrip
            {
                Dock = DockStyle.Top,
                Height = 0,
                BackColor = ColorPalette.Success,
            };
            Controls.Add(_toast);


            _sidebar = new SidebarPanel();
            Controls.Add(_sidebar);


            _centerPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColorPalette.BG };
            Controls.Add(_centerPanel);
            _centerPanel.BringToFront();

            _topBar = new TopBarPanel();
            _centerPanel.Controls.Add(_topBar);
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                BackColor = ColorPalette.Header,
            };
            _lblCacheStatus = new Label
            {
                Text = "Cache: trống",
                Font = new Font("Segoe UI", 8f),
                ForeColor = ColorPalette.SubText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent,
            };
            bottomBar.Controls.Add(_lblCacheStatus);
            _centerPanel.Controls.Add(bottomBar);


            _logPanel = new LogPanel();
            _centerPanel.Controls.Add(_logPanel);

            _statCard = new StatCard();
            _centerPanel.Controls.Add(_statCard);

            _grid = BuildGrid();
            _centerPanel.Controls.Add(_grid);
            _grid.BringToFront();
        }

        private DataGridView BuildGrid()
        {
            var dg = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ColorPalette.BG,
                GridColor = ColorPalette.Border,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 38,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 34 },
                Font = new Font("Segoe UI", 9.5f),
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };

            dg.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 34, 54),
                ForeColor = Color.FromArgb(200, 210, 255),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                SelectionBackColor = Color.FromArgb(30, 34, 54),
                SelectionForeColor = Color.FromArgb(200, 210, 255),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 0),
            };
            dg.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorPalette.BG,
                ForeColor = ColorPalette.Text,
                SelectionBackColor = ColorPalette.RowSel,
                SelectionForeColor = ColorPalette.Text,
                Padding = new Padding(4, 0, 4, 0),
            };
            dg.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorPalette.RowAlt,
                ForeColor = ColorPalette.Text,
                SelectionBackColor = ColorPalette.RowSel,
                SelectionForeColor = ColorPalette.Text,
                Padding = new Padding(4, 0, 4, 0),
            };
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MSSV",
                HeaderText = "MSSV",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HoTen",
                HeaderText = "Họ và Tên",
                FillWeight = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Lop",
                HeaderText = "Lớp",
                FillWeight = 18,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Diem",
                HeaderText = "Điểm TB",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });
            dg.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "XepLoai",
                HeaderText = "Xếp Loại",
                FillWeight = 17,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            });

            dg.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dg.Columns["XepLoai"].Index && e.Value != null)
                {
                    e.CellStyle.ForeColor = GradeService.GetGradeColor(e.Value.ToString());
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            };
            dg.SelectionChanged += (s, e) =>
            {
                if (dg.SelectedRows.Count == 0) return;
                var row = dg.SelectedRows[0];
                _selectedMSSV = row.Cells["MSSV"].Value?.ToString() ?? "";
                _sidebar.Fill(
                    _selectedMSSV,
                    row.Cells["HoTen"].Value?.ToString() ?? "",
                    row.Cells["Lop"].Value?.ToString() ?? "",
                    row.Cells["Diem"].Value?.ToString() ?? "");
            };

            return dg;
        }
        private void WireEvents()
        {
            _sidebar.BtnConnect.Click += OnConnect;
            _sidebar.BtnThem.Click += OnThem;
            _sidebar.BtnCapNhat.Click += OnCapNhat;
            _sidebar.BtnXoa.Click += OnXoa;
            _sidebar.BtnXoaForm.Click += (s, e) =>
            {
                _sidebar.Clear();
                _selectedMSSV = null;
                if (_grid.SelectedRows.Count > 0) _grid.ClearSelection();
            };

            _topBar.BtnLoad.Click += (s, e) => _ = LoadDataAsync();
            _topBar.BtnSort.Click += OnSort;
            _topBar.BtnExcel.Click += OnExportExcel;

            _topBar.TxtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; _ = ApplyFiltersAsync(); } };
            _topBar.TxtSearchDiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; _ = ApplyFiltersAsync(); } };
            _topBar.TxtSearchLop.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; _ = ApplyFiltersAsync(); } };
        }
        private async void OnConnect(object sender, EventArgs e) 
        {
            if (_conn.IsConnected)
            {
                _conn.Disconnect();
                _sidebar.SetConnected(false);
                ShowToast("Đã ngắt kết nối.", ColorPalette.Warn);
                return; 
            }
            try
            {
                ShowToast("Đang kết nối...", ColorPalette.Accent);
                await _conn.ConnectAsync(_sidebar.TxtHost.Text.Trim(), int.Parse(_sidebar.TxtPort.Text.Trim()));
                _sidebar.SetConnected(true);
                ShowToast("Kết nối thành công!", ColorPalette.Success);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _sidebar.SetConnected(false);
                ShowToast("Lỗi kết nối: " + ex.Message, ColorPalette.Danger);
            }
        }
        private async Task LoadDataAsync()
        {
            try
            {
                string resp = await _conn.SendAsync("GET\n"); 
                var items = ParseResponse(resp); 
                _cache.Update(items); 
                BindGrid(items); 
                _statCard.Hide();
                UpdateCacheLabel();
            }
           
            catch
            {
                if (_cache.HasData) 
                {
                    BindGrid(_cache.GetAll()); 
                    ShowToast("Mất kết nối — đang hiển thị dữ liệu cache.", ColorPalette.Warn);
                }
                else ShowToast("Không thể tải dữ liệu.", ColorPalette.Danger); 
            }
        }

        private async void OnSort(object sender, EventArgs e)
        {
            try
            {
                string dir = _topBar.SortAscending ? "asc" : "desc"; 
                string resp = await _conn.SendAsync($"GETSORTED;{dir}\n"); 
                var items = ParseResponse(resp); 
                _cache.Update(items); 
                BindGrid(items); 
                UpdateCacheLabel(); 
                ShowToast($"Đã sắp xếp theo Lớp {(dir == "asc" ? "A→Z" : "Z→A")}.", ColorPalette.Accent); // Thông báo thành công
            }
            catch (Exception ex)
            {
                
                var sorted = _cache.GetAll(); 
                sorted = _topBar.SortAscending 
                    ? sorted.OrderBy(x => x.Lop, StringComparer.OrdinalIgnoreCase).ToList() 
                    : sorted.OrderByDescending(x => x.Lop, StringComparer.OrdinalIgnoreCase).ToList(); 
                BindGrid(sorted); 
                ShowToast("Sort từ cache (offline): " + ex.Message, ColorPalette.Warn); 
            }
        }

        
        private async void OnThem(object sender, EventArgs e)
        {
            if (!ValidateFields(out string mssv, out string hoTen, out string lop, out float diem)) return; 
            try
            {
                string cmd = $"ADD;{mssv}|{hoTen}|{lop}|{diem.ToString(CultureInfo.InvariantCulture)}\n"; 
                string resp = await _conn.SendAsync(cmd); 
                if (resp.StartsWith("OK")) 
                {
                    var entry = _log.Add(LogAction.Them, $"{mssv} — {hoTen} — Lớp {lop} — Điểm {diem}"); 
                    _logPanel.AppendEntry(entry);
                    ShowToast("Thêm sinh viên thành công!", ColorPalette.Success);
                    _sidebar.Clear();
                    _selectedMSSV = null;
                    if (_grid.SelectedRows.Count > 0) _grid.ClearSelection();
                    await LoadDataAsync(); 
                }
                else ShowToast(resp.Replace("ERROR;", ""), ColorPalette.Danger);
            }
            catch (Exception ex) { ShowToast("Lỗi: " + ex.Message, ColorPalette.Danger); }
        }

        private async void OnCapNhat(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedMSSV)) 
            {
                ShowToast("Vui lòng chọn sinh viên từ danh sách để cập nhật.", ColorPalette.Warn);
                return;
            }

            if (!ValidateFields(out string mssv, out string hoTen, out string lop, out float diem)) return;

            if (mssv != _selectedMSSV) 
            {
                ShowToast("Không được phép thay đổi ID. Chỉ được đổi tên, lớp, điểm.", ColorPalette.Danger); 
                _sidebar.TxtMSSV.Text = _selectedMSSV; 
                return;
            }

            try
            {
                string cmd = $"UPDATE;{mssv}|{hoTen}|{lop}|{diem.ToString(CultureInfo.InvariantCulture)}\n"; 
                string resp = await _conn.SendAsync(cmd); 
                if (resp.StartsWith("OK")) 
                {
                    var entry = _log.Add(LogAction.CapNhat, $"{mssv} — {hoTen} — Lớp {lop} — Điểm {diem}");
                    _logPanel.AppendEntry(entry); 
                    ShowToast("Cập nhật thành công!", ColorPalette.Success); 
                }
                else { ShowToast(resp.Replace("ERROR;", ""), ColorPalette.Danger); return; } 
                _sidebar.Clear(); 
                _selectedMSSV = null; 
                if (_grid.SelectedRows.Count > 0) _grid.ClearSelection(); 
                await LoadDataAsync(); 
            }
            catch (Exception ex) { ShowToast("Lỗi: " + ex.Message, ColorPalette.Danger); }
        }

        private async void OnXoa(object sender, EventArgs e)
        {
            string mssv = _sidebar.TxtMSSV.Text.Trim();
            if (string.IsNullOrEmpty(mssv)) { ShowToast("Chưa chọn sinh viên cần xóa.", ColorPalette.Warn); return; } 

            if (MessageBox.Show($"Xóa sinh viên {mssv}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return; 
            try
            {
                string resp = await _conn.SendAsync($"DELETE;{mssv}\n"); 
                if (resp.StartsWith("OK")) 
                {
                    var entry = _log.Add(LogAction.Xoa, $"{mssv} — {_sidebar.TxtHoTen.Text.Trim()}"); 
                    _logPanel.AppendEntry(entry); 
                    ShowToast("Xóa thành công!", ColorPalette.Success); 
                    _sidebar.Clear(); 
                    _selectedMSSV = null; 
                    if (_grid.SelectedRows.Count > 0) _grid.ClearSelection(); 
                    await LoadDataAsync(); 
                }
                else ShowToast(resp.Replace("ERROR;", ""), ColorPalette.Danger); 
            }
            catch (Exception ex) { ShowToast("Lỗi: " + ex.Message, ColorPalette.Danger); } 
        }
        private async Task ApplyFiltersAsync()
        {
            string kw = _topBar.TxtSearch.Text.Trim(); 
            string lop = _topBar.TxtSearchLop.Text.Trim();
            string diemInput = _topBar.TxtSearchDiem.Text.Trim(); 

            float min = 0f, max = 10f; 
            if (!string.IsNullOrEmpty(diemInput)) 
            {
                diemInput = diemInput.Replace(" ", ""); 
                try
                {
                    //ex nhap 7-9 tach ra thanh min == 7 max = 9
                    if (diemInput.Contains("-")) 
                    {
                        var p = diemInput.Split('-'); 
                        min = float.Parse(p[0], CultureInfo.InvariantCulture); 
                        max = float.Parse(p[1], CultureInfo.InvariantCulture); 
                    }
                    else if (diemInput.StartsWith(">=")) 
                        min = float.Parse(diemInput.Substring(2), CultureInfo.InvariantCulture); 
                    else if (diemInput.StartsWith("<=")) 
                        max = float.Parse(diemInput.Substring(2), CultureInfo.InvariantCulture); 
                    else if (diemInput.StartsWith(">"))
                        min = float.Parse(diemInput.Substring(1), CultureInfo.InvariantCulture) + 0.01f; 
                    else if (diemInput.StartsWith("<")) 
                        max = float.Parse(diemInput.Substring(1), CultureInfo.InvariantCulture) - 0.01f; 
                    else
                        min = max = float.Parse(diemInput, CultureInfo.InvariantCulture); 
                }
                catch
                {
                    ShowToast("Định dạng điểm không hợp lệ. Vd: 7.0-9.0", ColorPalette.Warn); 
                    return; 
                }
            }

            try
            {
                string cmd = $"SEARCHMULTI;{kw}|{min.ToString(CultureInfo.InvariantCulture)}|{max.ToString(CultureInfo.InvariantCulture)}|{lop}\n"; 
                string resp = await _conn.SendAsync(cmd); 
                var items = ParseResponse(resp); 
                _cache.Update(items); 
                BindGrid(items); 
                if (!string.IsNullOrEmpty(lop)) _statCard.Update(lop, items); 
                else _statCard.Hide(); 
                UpdateCacheLabel(); 
                ShowToast($"Đã tìm thấy {items.Count} sinh viên thỏa mãn điều kiện.", ColorPalette.Accent); 
            }
            catch (Exception ex)
            {
                ShowToast("Lỗi tìm kiếm: " + ex.Message, ColorPalette.Danger);
            }
        }

        
        private void OnExportExcel(object sender, EventArgs e)
       
        {
            var items = _cache.GetAll();
            if (items.Count == 0) { ShowToast("Không có dữ liệu để xuất.", ColorPalette.Warn); return; }

            using (var dlg = new SaveFileDialog
            {
                Title = "Xuất danh sách sinh viên",
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"SinhVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
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
                    MSSV = p[0].Trim(),
                    HoTen = p[1].Trim(),
                    Lop = p[2].Trim(),
                    Diem = d.ToString("F2", CultureInfo.InvariantCulture),
                    XepLoai = GradeService.GetGrade(d),
                });
            }
            return result;
        }
        private void BindGrid(List<SinhVienItem> items)
        {
            if (InvokeRequired) { Invoke((Action)(() => BindGrid(items))); return; }
            _grid.Rows.Clear();
            foreach (var sv in items)
                _grid.Rows.Add(sv.MSSV, sv.HoTen, sv.Lop, sv.Diem, sv.XepLoai);
        }
        private bool ValidateFields(out string mssv, out string hoTen, out string lop, out float diem)
        {
            mssv = _sidebar.TxtMSSV.Text.Trim();
            hoTen = _sidebar.TxtHoTen.Text.Trim();
            lop = _sidebar.TxtLop.Text.Trim();
            diem = 0;

            if (string.IsNullOrEmpty(mssv)) { ShowToast("Vui lòng nhập MSSV.", ColorPalette.Warn); return false; }
            if (string.IsNullOrEmpty(hoTen)) { ShowToast("Vui lòng nhập Họ tên.", ColorPalette.Warn); return false; }
            if (string.IsNullOrEmpty(lop)) { ShowToast("Vui lòng nhập Lớp.", ColorPalette.Warn); return false; }
            if (!float.TryParse(_sidebar.TxtDiem.Text.Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out diem) || diem < 0 || diem > 10)
            {
                ShowToast("Điểm không hợp lệ (0 – 10).", ColorPalette.Warn); return false;
            }
            return true;
        }
        private void ShowToast(string msg, Color color)
        {
            if (InvokeRequired) { Invoke((Action)(() => ShowToast(msg, color))); return; }
            _toast.Msg = msg;
            _toast.BackColor = color;
            _toast.Height = 28;
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

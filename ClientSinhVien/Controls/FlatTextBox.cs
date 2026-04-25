using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClientSinhVien.Controls
{
    /// <summary>
    /// File: FlatTextBox.cs
    /// Chức năng: Custom Control hộp thoại nhập liệu (TextBox) với giao diện bo tròn.
    /// Tính năng:
    /// - Hỗ trợ PlaceholderText (hiển thị chữ mờ hướng dẫn khi trống).
    /// - Vẽ lại đường viền (Border) và màu nền (Background) hiện đại hơn so với TextBox mặc định của WinForms.
    /// </summary>
    public class FlatTextBox : Panel
    {
        private readonly TextBox _tb;

        public Color  BgColor  { get => BackColor;     set { BackColor = value; _tb.BackColor = value; } }
        public Color  TxtColor { get => _tb.ForeColor; set => _tb.ForeColor = value; }
        public Color  BdColor  { get; set; } = Color.FromArgb(80, 80, 120);
        public int    Radius   { get; set; } = 8;
        public string PlaceholderText { get; set; } = "";
        public new string Text 
        { 
            get => (_tb.Text == PlaceholderText) ? "" : _tb.Text; 
            set => _tb.Text = string.IsNullOrEmpty(value) ? PlaceholderText : value; 
        }

        public new event System.EventHandler    Enter   { add => _tb.Enter   += value; remove => _tb.Enter   -= value; }
        public new event System.EventHandler    Leave   { add => _tb.Leave   += value; remove => _tb.Leave   -= value; }
        public new event KeyEventHandler        KeyDown { add => _tb.KeyDown += value; remove => _tb.KeyDown -= value; }

        public FlatTextBox()
        {
            _tb = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9.5f) };
            Controls.Add(_tb);
            Padding  = new Padding(10, 0, 10, 0);
            Paint   += (s, e) => Draw(e.Graphics);
            Resize  += (s, e) => LayoutTb();
            LayoutTb();
            
            _tb.Enter += (s, e) => { if (_tb.Text == PlaceholderText) _tb.Text = ""; };
            _tb.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_tb.Text)) _tb.Text = PlaceholderText; };
        }

        private void LayoutTb()
        {
            _tb.Location = new Point(Padding.Left, (Height - _tb.Height) / 2);
            _tb.Width    = Width - Padding.Left - Padding.Right;
        }

        private void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
            {
                g.FillPath(new SolidBrush(BackColor), path);
                g.DrawPath(new Pen(BdColor, 1.5f), path);
            }
        }

        public static GraphicsPath RoundRect(Rectangle r, int d)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X,             r.Y,              d * 2, d * 2, 180, 90);
            p.AddArc(r.Right - d * 2, r.Y,              d * 2, d * 2, 270, 90);
            p.AddArc(r.Right - d * 2, r.Bottom - d * 2, d * 2, d * 2,   0, 90);
            p.AddArc(r.X,             r.Bottom - d * 2, d * 2, d * 2,  90, 90);
            p.CloseFigure();
            return p;
        }
    }
}

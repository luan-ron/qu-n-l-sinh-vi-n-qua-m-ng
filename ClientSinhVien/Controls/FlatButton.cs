using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClientSinhVien.Controls
{
    /// <summary>Rounded button với hover/press states.</summary>
    public class FlatButton : Control
    {
        public Color BgColor  { get; set; }
        public Color TxtColor { get; set; } = Color.White;
        public int   Radius   { get; set; } = 8;

        private bool _hov, _prs;

        public FlatButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(System.EventArgs e) { _hov = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(System.EventArgs e) { _hov = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e)    { _prs = true;  Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e)      { _prs = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill = !Enabled ? Color.FromArgb(50, 50, 72)
                       : _prs    ? ControlPaint.Dark(BgColor,  0.15f)
                       : _hov    ? ControlPaint.Light(BgColor, 0.10f)
                       : BgColor;

            using (var path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
                g.FillPath(new SolidBrush(fill), path);

            Color tc = Enabled ? TxtColor : Color.FromArgb(100, 100, 120);
            TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height), tc,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
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

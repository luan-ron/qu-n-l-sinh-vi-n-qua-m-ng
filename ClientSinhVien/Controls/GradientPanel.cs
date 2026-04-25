using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClientSinhVien.Controls
{
    /// <summary>Panel với linear gradient ngang.</summary>
    public class GradientPanel : Panel
    {
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }

        public GradientPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;
            using (var b = new LinearGradientBrush(
                ClientRectangle, Color1, Color2, LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(b, ClientRectangle);
        }
    }
}

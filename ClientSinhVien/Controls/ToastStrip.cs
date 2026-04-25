using System.Drawing;
using System.Windows.Forms;

namespace ClientSinhVien.Controls
{
    /// <summary>Thanh thông báo mỏng docked top, tự ẩn sau 3 giây.</summary>
    public class ToastStrip : Panel
    {
        public string Msg { get; set; } = "";

        public ToastStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            TextRenderer.DrawText(e.Graphics, "   " + Msg,
                new Font("Segoe UI", 9f, FontStyle.Bold),
                new Rectangle(0, 0, Width, Height),
                Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine);
        }
    }
}

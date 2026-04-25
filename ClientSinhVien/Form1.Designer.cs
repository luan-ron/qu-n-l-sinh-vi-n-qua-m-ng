namespace ClientSinhVien
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Form1 không dùng designer-generated controls.
        /// Toàn bộ UI được khởi tạo trong BuildUI() bên Form1.cs.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1180, 720);
            this.Name                = "Form1";
            this.Text                = "Quản Lý Sinh Viên qua Mạng";
            this.ResumeLayout(false);
        }
    }
}

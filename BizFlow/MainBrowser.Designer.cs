namespace BizFlow
{
    partial class MainBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainWebBrowser = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.MainWebBrowser)).BeginInit();
            this.SuspendLayout();
            // 
            // MainWebBrowser
            // 
            this.MainWebBrowser.AllowExternalDrop = true;
            this.MainWebBrowser.CreationProperties = null;
            this.MainWebBrowser.DefaultBackgroundColor = System.Drawing.Color.White;
            this.MainWebBrowser.Location = new System.Drawing.Point(12, 12);
            this.MainWebBrowser.Name = "MainWebBrowser";
            this.MainWebBrowser.Size = new System.Drawing.Size(776, 426);
            this.MainWebBrowser.TabIndex = 0;
            this.MainWebBrowser.ZoomFactor = 1D;
            // 
            // MainBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.MainWebBrowser);
            this.Name = "MainBrowser";
            this.Text = "MainBrowser";
            ((System.ComponentModel.ISupportInitialize)(this.MainWebBrowser)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 MainWebBrowser;
    }
}
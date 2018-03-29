namespace WindowsFormsApplication1
{
    partial class ChooseDriver
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
            this.ChooseDriver_cbx = new System.Windows.Forms.ComboBox();
            this.queding_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChooseDriver_cbx
            // 
            this.ChooseDriver_cbx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ChooseDriver_cbx.FormattingEnabled = true;
            this.ChooseDriver_cbx.Location = new System.Drawing.Point(61, 59);
            this.ChooseDriver_cbx.Name = "ChooseDriver_cbx";
            this.ChooseDriver_cbx.Size = new System.Drawing.Size(180, 20);
            this.ChooseDriver_cbx.TabIndex = 0;
            // 
            // queding_btn
            // 
            this.queding_btn.Location = new System.Drawing.Point(102, 175);
            this.queding_btn.Name = "queding_btn";
            this.queding_btn.Size = new System.Drawing.Size(112, 27);
            this.queding_btn.TabIndex = 1;
            this.queding_btn.Text = "button1";
            this.queding_btn.UseVisualStyleBackColor = true;
            this.queding_btn.Click += new System.EventHandler(this.queding_btn_Click);
            // 
            // ChooseDriver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 269);
            this.Controls.Add(this.queding_btn);
            this.Controls.Add(this.ChooseDriver_cbx);
            this.Name = "ChooseDriver";
            this.Text = "ChooseDriver";
            this.Load += new System.EventHandler(this.ChooseDriver_Load);
            this.ResumeLayout(false);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChooseDriver_FormClosing);
            //this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChooseDriver_FormClosed);

        }

        #endregion

        private System.Windows.Forms.ComboBox ChooseDriver_cbx;
        private System.Windows.Forms.Button queding_btn;
    }
}
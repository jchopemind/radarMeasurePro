namespace WindowsFormsApplication1
{
    partial class deviceChoose
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(deviceChoose));
            this.deviceChoose_cbo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // deviceChoose_cbo
            // 
            this.deviceChoose_cbo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceChoose_cbo.FormattingEnabled = true;
            this.deviceChoose_cbo.Items.AddRange(new object[] {
            "LMS1xx",
            "LMS5xx"});
            this.deviceChoose_cbo.Location = new System.Drawing.Point(61, 43);
            this.deviceChoose_cbo.Name = "deviceChoose_cbo";
            this.deviceChoose_cbo.Size = new System.Drawing.Size(121, 20);
            this.deviceChoose_cbo.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(25, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "请选择雷达型号：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(161, 85);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "确认选择";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // deviceChoose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(258, 125);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.deviceChoose_cbo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "deviceChoose";
            this.Text = "雷达选择";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.deviceChoose_FormClosed);
            this.Load += new System.EventHandler(this.deiceChoose_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox deviceChoose_cbo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}
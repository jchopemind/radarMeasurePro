namespace WindowsFormsApplication1
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dtcl_bto = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.jqcl_bto = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(-1, -3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(986, 125);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // dtcl_bto
            // 
            this.dtcl_bto.Font = new System.Drawing.Font("宋体", 25F);
            this.dtcl_bto.Location = new System.Drawing.Point(198, 230);
            this.dtcl_bto.Name = "dtcl_bto";
            this.dtcl_bto.Size = new System.Drawing.Size(179, 92);
            this.dtcl_bto.TabIndex = 1;
            this.dtcl_bto.Text = "动态测量";
            this.dtcl_bto.UseVisualStyleBackColor = true;
            this.dtcl_bto.Click += new System.EventHandler(this.dtcl_bto_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 25F);
            this.label1.Location = new System.Drawing.Point(247, 400);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(491, 34);
            this.label1.TabIndex = 3;
            this.label1.Text = "四川瑞恩智铁电气设备有限公司";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("宋体", 20F);
            this.button3.Location = new System.Drawing.Point(812, 481);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(110, 60);
            this.button3.TabIndex = 4;
            this.button3.Text = "退出";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // jqcl_bto
            // 
            this.jqcl_bto.Font = new System.Drawing.Font("宋体", 25F);
            this.jqcl_bto.Location = new System.Drawing.Point(609, 230);
            this.jqcl_bto.Name = "jqcl_bto";
            this.jqcl_bto.Size = new System.Drawing.Size(171, 92);
            this.jqcl_bto.TabIndex = 2;
            this.jqcl_bto.Text = "精确测量";
            this.jqcl_bto.UseVisualStyleBackColor = true;
            this.jqcl_bto.Click += new System.EventHandler(this.jqcl_bto_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 586);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.jqcl_bto);
            this.Controls.Add(this.dtcl_bto);
            this.Controls.Add(this.pictureBox1);
            this.MaximumSize = new System.Drawing.Size(1000, 625);
            this.MinimumSize = new System.Drawing.Size(1000, 625);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button dtcl_bto;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button jqcl_bto;
    }
}


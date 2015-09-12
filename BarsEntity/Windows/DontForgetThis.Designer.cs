namespace Barsix.BarsEntity
{
    partial class DontForgetThis
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DontForgetThis));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnCopyClipboard = new System.Windows.Forms.Button();
            this.btnInsertFragments = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(13, 44);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(560, 320);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // btnCopyClipboard
            // 
            this.btnCopyClipboard.Location = new System.Drawing.Point(13, 13);
            this.btnCopyClipboard.Name = "btnCopyClipboard";
            this.btnCopyClipboard.Size = new System.Drawing.Size(179, 25);
            this.btnCopyClipboard.TabIndex = 1;
            this.btnCopyClipboard.Text = "В буфер обмена";
            this.btnCopyClipboard.UseVisualStyleBackColor = true;
            this.btnCopyClipboard.Click += new System.EventHandler(this.btnCopyClipboard_Click);
            // 
            // btnInsertFragments
            // 
            this.btnInsertFragments.Location = new System.Drawing.Point(377, 12);
            this.btnInsertFragments.Name = "btnInsertFragments";
            this.btnInsertFragments.Size = new System.Drawing.Size(196, 26);
            this.btnInsertFragments.TabIndex = 2;
            this.btnInsertFragments.Text = "Вставить в файлы";
            this.btnInsertFragments.UseVisualStyleBackColor = true;
            this.btnInsertFragments.Click += new System.EventHandler(this.btnInsertFragments_Click);
            // 
            // DontForgetThis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 376);
            this.Controls.Add(this.btnInsertFragments);
            this.Controls.Add(this.btnCopyClipboard);
            this.Controls.Add(this.richTextBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DontForgetThis";
            this.ShowInTaskbar = false;
            this.Text = "Осталось добавить эти строки";
            this.Load += new System.EventHandler(this.DontForgetThis_Load);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnCopyClipboard;
        private System.Windows.Forms.Button btnInsertFragments;

    }
}
namespace Barsix.BarsEntity.Windows
{
    partial class ClassBrowserWindow
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
            this.chState = new System.Windows.Forms.CheckBox();
            this.chSign = new System.Windows.Forms.CheckBox();
            this.chTree = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.cbNamespace = new System.Windows.Forms.ComboBox();
            this.lbMembers = new System.Windows.Forms.ListView();
            this.Название = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Тип = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // chState
            // 
            this.chState.AutoSize = true;
            this.chState.Location = new System.Drawing.Point(15, 319);
            this.chState.Name = "chState";
            this.chState.Size = new System.Drawing.Size(60, 17);
            this.chState.TabIndex = 4;
            this.chState.Text = "Статус";
            this.chState.UseVisualStyleBackColor = true;
            // 
            // chSign
            // 
            this.chSign.AutoSize = true;
            this.chSign.Location = new System.Drawing.Point(15, 343);
            this.chSign.Name = "chSign";
            this.chSign.Size = new System.Drawing.Size(49, 17);
            this.chSign.TabIndex = 5;
            this.chSign.Text = "ЭЦП";
            this.chSign.UseVisualStyleBackColor = true;
            // 
            // chTree
            // 
            this.chTree.AutoSize = true;
            this.chTree.Location = new System.Drawing.Point(15, 367);
            this.chTree.Name = "chTree";
            this.chTree.Size = new System.Drawing.Size(104, 17);
            this.chTree.TabIndex = 6;
            this.chTree.Text = "Иерархический";
            this.chTree.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(241, 349);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(112, 36);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Выбрать";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cbNamespace
            // 
            this.cbNamespace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbNamespace.FormattingEnabled = true;
            this.cbNamespace.Location = new System.Drawing.Point(12, 22);
            this.cbNamespace.Name = "cbNamespace";
            this.cbNamespace.Size = new System.Drawing.Size(341, 21);
            this.cbNamespace.TabIndex = 8;
            this.cbNamespace.SelectedIndexChanged += new System.EventHandler(this.cbNamespace_SelectedIndexChanged);
            // 
            // lbMembers
            // 
            this.lbMembers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Название,
            this.Тип});
            this.lbMembers.FullRowSelect = true;
            this.lbMembers.Location = new System.Drawing.Point(12, 49);
            this.lbMembers.Name = "lbMembers";
            this.lbMembers.Size = new System.Drawing.Size(341, 264);
            this.lbMembers.TabIndex = 9;
            this.lbMembers.UseCompatibleStateImageBehavior = false;
            this.lbMembers.View = System.Windows.Forms.View.Details;
            // 
            // Название
            // 
            this.Название.Text = "Название";
            this.Название.Width = 150;
            // 
            // Тип
            // 
            this.Тип.Text = "Тип";
            this.Тип.Width = 180;
            // 
            // ClassBrowserWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 397);
            this.Controls.Add(this.lbMembers);
            this.Controls.Add(this.cbNamespace);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chTree);
            this.Controls.Add(this.chSign);
            this.Controls.Add(this.chState);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ClassBrowserWindow";
            this.Text = "Выбор класса";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chState;
        private System.Windows.Forms.CheckBox chSign;
        private System.Windows.Forms.CheckBox chTree;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cbNamespace;
        private System.Windows.Forms.ListView lbMembers;
        private System.Windows.Forms.ColumnHeader Название;
        private System.Windows.Forms.ColumnHeader Тип;
    }
}
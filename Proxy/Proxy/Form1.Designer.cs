namespace Proxy
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.logsListView = new System.Windows.Forms.ListView();
            this.logColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.configButton = new System.Windows.Forms.Button();
            this.startProxyButton = new System.Windows.Forms.Button();
            this.connectElectionAuthorityButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // logsListView
            // 
            this.logsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.logColumn});
            this.logsListView.FullRowSelect = true;
            this.logsListView.Location = new System.Drawing.Point(13, 83);
            this.logsListView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.logsListView.Name = "logsListView";
            this.logsListView.Size = new System.Drawing.Size(505, 270);
            this.logsListView.TabIndex = 0;
            this.logsListView.UseCompatibleStateImageBehavior = false;
            this.logsListView.View = System.Windows.Forms.View.Details;
            // 
            // logColumn
            // 
            this.logColumn.Text = "";
            this.logColumn.Width = 495;
            // 
            // configButton
            // 
            this.configButton.Location = new System.Drawing.Point(553, 83);
            this.configButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(143, 62);
            this.configButton.TabIndex = 1;
            this.configButton.Text = "Wczytaj konfigurację";
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.configButton_Click);
            // 
            // startProxyButton
            // 
            this.startProxyButton.Enabled = false;
            this.startProxyButton.Location = new System.Drawing.Point(552, 180);
            this.startProxyButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.startProxyButton.Name = "startProxyButton";
            this.startProxyButton.Size = new System.Drawing.Size(144, 69);
            this.startProxyButton.TabIndex = 2;
            this.startProxyButton.Text = "START";
            this.startProxyButton.UseVisualStyleBackColor = true;
            this.startProxyButton.Click += new System.EventHandler(this.startProxyButton_Click);
            // 
            // connectElectionAuthorityButton
            // 
            this.connectElectionAuthorityButton.Enabled = false;
            this.connectElectionAuthorityButton.Location = new System.Drawing.Point(553, 284);
            this.connectElectionAuthorityButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.connectElectionAuthorityButton.Name = "connectElectionAuthorityButton";
            this.connectElectionAuthorityButton.Size = new System.Drawing.Size(143, 69);
            this.connectElectionAuthorityButton.TabIndex = 3;
            this.connectElectionAuthorityButton.Text = "Połącz z EA";
            this.connectElectionAuthorityButton.UseVisualStyleBackColor = true;
            this.connectElectionAuthorityButton.Click += new System.EventHandler(this.connectElectionAuthorityButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(297, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 48);
            this.label1.TabIndex = 4;
            this.label1.Text = "Proxy";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(12, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Dziennik logów";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.OliveDrab;
            this.ClientSize = new System.Drawing.Size(717, 374);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectElectionAuthorityButton);
            this.Controls.Add(this.startProxyButton);
            this.Controls.Add(this.configButton);
            this.Controls.Add(this.logsListView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Proxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView logsListView;
        private System.Windows.Forms.ColumnHeader logColumn;
        private System.Windows.Forms.Button configButton;
        private System.Windows.Forms.Button startProxyButton;
        private System.Windows.Forms.Button connectElectionAuthorityButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}


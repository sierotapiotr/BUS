namespace ElectionAuthority
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
            this.loadConfigButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.proxyButton = new System.Windows.Forms.Button();
            this.endButton = new System.Windows.Forms.Button();
            this.countButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.logLabel = new System.Windows.Forms.Label();
            this.confOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.loadCandidatesButton = new System.Windows.Forms.Button();
            this.candOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.logListView = new System.Windows.Forms.ListView();
            this.logColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // loadConfigButton
            // 
            resources.ApplyResources(this.loadConfigButton, "loadConfigButton");
            this.loadConfigButton.Name = "loadConfigButton";
            this.loadConfigButton.UseVisualStyleBackColor = true;
            this.loadConfigButton.Click += new System.EventHandler(this.loadConfigButton_Click);
            // 
            // startButton
            // 
            resources.ApplyResources(this.startButton, "startButton");
            this.startButton.Name = "startButton";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // proxyButton
            // 
            resources.ApplyResources(this.proxyButton, "proxyButton");
            this.proxyButton.Name = "proxyButton";
            this.proxyButton.UseVisualStyleBackColor = true;
            this.proxyButton.Click += new System.EventHandler(this.proxyButton_Click);
            // 
            // endButton
            // 
            resources.ApplyResources(this.endButton, "endButton");
            this.endButton.Name = "endButton";
            this.endButton.UseVisualStyleBackColor = true;
            this.endButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // countButton
            // 
            resources.ApplyResources(this.countButton, "countButton");
            this.countButton.Name = "countButton";
            this.countButton.UseVisualStyleBackColor = true;
            this.countButton.Click += new System.EventHandler(this.countButton_Click);
            // 
            // titleLabel
            // 
            resources.ApplyResources(this.titleLabel, "titleLabel");
            this.titleLabel.Name = "titleLabel";
            // 
            // logLabel
            // 
            resources.ApplyResources(this.logLabel, "logLabel");
            this.logLabel.Name = "logLabel";
            // 
            // confOpenFileDialog
            // 
            this.confOpenFileDialog.FileName = "openFileDialog1";
            this.confOpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.confOpenFileDialog_FileOk);
            // 
            // loadCandidatesButton
            // 
            resources.ApplyResources(this.loadCandidatesButton, "loadCandidatesButton");
            this.loadCandidatesButton.Name = "loadCandidatesButton";
            this.loadCandidatesButton.UseVisualStyleBackColor = true;
            this.loadCandidatesButton.Click += new System.EventHandler(this.loadCandidatesButton_Click);
            // 
            // candOpenFileDialog
            // 
            this.candOpenFileDialog.FileName = "openFileDialog1";
            this.candOpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.candOpenFileDialog_FileOk);
            // 
            // logListView
            // 
            this.logListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.logColumn});
            this.logListView.FullRowSelect = true;
            resources.ApplyResources(this.logListView, "logListView");
            this.logListView.Name = "logListView";
            this.logListView.UseCompatibleStateImageBehavior = false;
            this.logListView.View = System.Windows.Forms.View.Details;
            // 
            // logColumn
            // 
            resources.ApplyResources(this.logColumn, "logColumn");
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.logListView);
            this.Controls.Add(this.loadCandidatesButton);
            this.Controls.Add(this.logLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.countButton);
            this.Controls.Add(this.endButton);
            this.Controls.Add(this.proxyButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.loadConfigButton);
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadConfigButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button proxyButton;
        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.Button countButton;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.OpenFileDialog confOpenFileDialog;
        private System.Windows.Forms.Button loadCandidatesButton;
        private System.Windows.Forms.OpenFileDialog candOpenFileDialog;
        private System.Windows.Forms.ListView logListView;
        private System.Windows.Forms.ColumnHeader logColumn;
    }
}


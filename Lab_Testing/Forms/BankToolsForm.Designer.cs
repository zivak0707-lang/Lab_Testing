namespace Lab_Testing
{
    partial class BankToolsForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlTop = new Panel();
            lblTitle = new Label();
            grpBank = new GroupBox();
            btnCheckFile = new Button();
            btnSaveTemplate = new Button();
            btnSaveJson = new Button();
            btnPrepare = new Button();
            rbFileBank = new RadioButton();
            rbCurrentBank = new RadioButton();
            lblBankInfo = new Label();
            grpKey = new GroupBox();
            btnImportKey = new Button();
            btnExportKey = new Button();
            lblKeyInfo = new Label();
            lblLogTitle = new Label();
            rtbLog = new RichTextBox();
            openJsonDialog = new OpenFileDialog();
            saveJsonDialog = new SaveFileDialog();
            openKeyDialog = new OpenFileDialog();
            saveKeyDialog = new SaveFileDialog();
            folderDialog = new FolderBrowserDialog();
            pnlTop.SuspendLayout();
            grpBank.SuspendLayout();
            grpKey.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.BackColor = Color.FromArgb(31, 78, 121);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(780, 58);
            pnlTop.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(18, 13);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(291, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Банк питань і ключ підпису";
            //
            // grpBank
            //
            grpBank.Controls.Add(btnCheckFile);
            grpBank.Controls.Add(btnSaveTemplate);
            grpBank.Controls.Add(btnSaveJson);
            grpBank.Controls.Add(btnPrepare);
            grpBank.Controls.Add(rbFileBank);
            grpBank.Controls.Add(rbCurrentBank);
            grpBank.Controls.Add(lblBankInfo);
            grpBank.Location = new Point(20, 72);
            grpBank.Name = "grpBank";
            grpBank.Size = new Size(740, 200);
            grpBank.TabIndex = 1;
            grpBank.TabStop = false;
            grpBank.Text = "Банк питань";
            //
            // btnCheckFile
            //
            btnCheckFile.Location = new Point(470, 152);
            btnCheckFile.Name = "btnCheckFile";
            btnCheckFile.Size = new Size(254, 34);
            btnCheckFile.TabIndex = 6;
            btnCheckFile.Text = "Перевірити файл банку…";
            btnCheckFile.UseVisualStyleBackColor = true;
            btnCheckFile.Click += BtnCheckFile_Click;
            //
            // btnSaveTemplate
            //
            btnSaveTemplate.Location = new Point(258, 152);
            btnSaveTemplate.Name = "btnSaveTemplate";
            btnSaveTemplate.Size = new Size(202, 34);
            btnSaveTemplate.TabIndex = 5;
            btnSaveTemplate.Text = "Зберегти шаблон банку…";
            btnSaveTemplate.UseVisualStyleBackColor = true;
            btnSaveTemplate.Click += BtnSaveTemplate_Click;
            //
            // btnSaveJson
            //
            btnSaveJson.Location = new Point(16, 152);
            btnSaveJson.Name = "btnSaveJson";
            btnSaveJson.Size = new Size(232, 34);
            btnSaveJson.TabIndex = 4;
            btnSaveJson.Text = "Зберегти банк для редагування…";
            btnSaveJson.UseVisualStyleBackColor = true;
            btnSaveJson.Click += BtnSaveJson_Click;
            //
            // btnPrepare
            //
            btnPrepare.BackColor = Color.FromArgb(46, 125, 50);
            btnPrepare.Cursor = Cursors.Hand;
            btnPrepare.FlatAppearance.BorderSize = 0;
            btnPrepare.FlatStyle = FlatStyle.Flat;
            btnPrepare.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnPrepare.ForeColor = Color.White;
            btnPrepare.Location = new Point(16, 100);
            btnPrepare.Name = "btnPrepare";
            btnPrepare.Size = new Size(708, 40);
            btnPrepare.TabIndex = 3;
            btnPrepare.Text = "Підготувати банк для учнів (questions.dat + ZIP)…";
            btnPrepare.UseVisualStyleBackColor = false;
            btnPrepare.Click += BtnPrepare_Click;
            //
            // rbFileBank
            //
            rbFileBank.AutoSize = true;
            rbFileBank.Location = new Point(250, 70);
            rbFileBank.Name = "rbFileBank";
            rbFileBank.Size = new Size(260, 23);
            rbFileBank.TabIndex = 2;
            rbFileBank.Text = "банк з файлу JSON, який ви редагували";
            rbFileBank.UseVisualStyleBackColor = true;
            //
            // rbCurrentBank
            //
            rbCurrentBank.AutoSize = true;
            rbCurrentBank.Checked = true;
            rbCurrentBank.Location = new Point(18, 70);
            rbCurrentBank.Name = "rbCurrentBank";
            rbCurrentBank.Size = new Size(200, 23);
            rbCurrentBank.TabIndex = 1;
            rbCurrentBank.TabStop = true;
            rbCurrentBank.Text = "чинний банк програми";
            rbCurrentBank.UseVisualStyleBackColor = true;
            //
            // lblBankInfo
            //
            lblBankInfo.Location = new Point(16, 26);
            lblBankInfo.Name = "lblBankInfo";
            lblBankInfo.Size = new Size(708, 40);
            lblBankInfo.TabIndex = 0;
            lblBankInfo.Text = "Чинний банк: —";
            //
            // grpKey
            //
            grpKey.Controls.Add(btnImportKey);
            grpKey.Controls.Add(btnExportKey);
            grpKey.Controls.Add(lblKeyInfo);
            grpKey.Location = new Point(20, 282);
            grpKey.Name = "grpKey";
            grpKey.Size = new Size(740, 122);
            grpKey.TabIndex = 2;
            grpKey.TabStop = false;
            grpKey.Text = "Ключ підпису вчителя";
            //
            // btnImportKey
            //
            btnImportKey.Location = new Point(258, 76);
            btnImportKey.Name = "btnImportKey";
            btnImportKey.Size = new Size(232, 34);
            btnImportKey.TabIndex = 2;
            btnImportKey.Text = "Відновити ключ з файлу…";
            btnImportKey.UseVisualStyleBackColor = true;
            btnImportKey.Click += BtnImportKey_Click;
            //
            // btnExportKey
            //
            btnExportKey.Location = new Point(16, 76);
            btnExportKey.Name = "btnExportKey";
            btnExportKey.Size = new Size(232, 34);
            btnExportKey.TabIndex = 1;
            btnExportKey.Text = "Зберегти копію ключа…";
            btnExportKey.UseVisualStyleBackColor = true;
            btnExportKey.Click += BtnExportKey_Click;
            //
            // lblKeyInfo
            //
            lblKeyInfo.Location = new Point(16, 26);
            lblKeyInfo.Name = "lblKeyInfo";
            lblKeyInfo.Size = new Size(708, 44);
            lblKeyInfo.TabIndex = 0;
            lblKeyInfo.Text = "Ключ: —";
            //
            // lblLogTitle
            //
            lblLogTitle.AutoSize = true;
            lblLogTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLogTitle.ForeColor = Color.FromArgb(31, 78, 121);
            lblLogTitle.Location = new Point(18, 414);
            lblLogTitle.Name = "lblLogTitle";
            lblLogTitle.Size = new Size(96, 19);
            lblLogTitle.TabIndex = 3;
            lblLogTitle.Text = "Що зроблено";
            //
            // rtbLog
            //
            rtbLog.BackColor = Color.White;
            rtbLog.DetectUrls = false;
            rtbLog.Location = new Point(20, 438);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbLog.Size = new Size(740, 150);
            rtbLog.TabIndex = 4;
            rtbLog.Text = "";
            //
            // openJsonDialog
            //
            openJsonDialog.Filter = "Банк питань (*.json)|*.json|Усі файли (*.*)|*.*";
            openJsonDialog.Title = "Оберіть файл банку питань";
            //
            // saveJsonDialog
            //
            saveJsonDialog.DefaultExt = "json";
            saveJsonDialog.Filter = "Банк питань (*.json)|*.json";
            saveJsonDialog.Title = "Зберегти банк питань";
            //
            // openKeyDialog
            //
            openKeyDialog.Filter = "Ключ вчителя (*.labkey)|*.labkey";
            openKeyDialog.Title = "Оберіть файл копії ключа";
            //
            // saveKeyDialog
            //
            saveKeyDialog.DefaultExt = "labkey";
            saveKeyDialog.Filter = "Ключ вчителя (*.labkey)|*.labkey";
            saveKeyDialog.Title = "Зберегти копію ключа";
            //
            // folderDialog
            //
            folderDialog.Description = "Оберіть теку, куди зберегти файли для учнів";
            folderDialog.UseDescriptionForTitle = true;
            //
            // BankToolsForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(780, 606);
            Controls.Add(rtbLog);
            Controls.Add(lblLogTitle);
            Controls.Add(grpKey);
            Controls.Add(grpBank);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BankToolsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Банк питань і ключ підпису";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            grpBank.ResumeLayout(false);
            grpBank.PerformLayout();
            grpKey.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlTop;
        private Label lblTitle;
        private GroupBox grpBank;
        private Label lblBankInfo;
        private RadioButton rbCurrentBank;
        private RadioButton rbFileBank;
        private Button btnPrepare;
        private Button btnSaveJson;
        private Button btnSaveTemplate;
        private Button btnCheckFile;
        private GroupBox grpKey;
        private Label lblKeyInfo;
        private Button btnExportKey;
        private Button btnImportKey;
        private Label lblLogTitle;
        private RichTextBox rtbLog;
        private OpenFileDialog openJsonDialog;
        private SaveFileDialog saveJsonDialog;
        private OpenFileDialog openKeyDialog;
        private SaveFileDialog saveKeyDialog;
        private FolderBrowserDialog folderDialog;
    }
}

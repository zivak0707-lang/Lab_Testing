namespace Lab_Testing
{
    partial class TeacherForm
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
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle noteStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle violationStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle nameStyle = new DataGridViewCellStyle();
            pnlTop = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            pnlToolbar = new Panel();
            btnBankTools = new Button();
            pnlPasswordHint = new Panel();
            btnCloseHint = new Button();
            lnkChangePassword = new LinkLabel();
            lblPasswordHint = new Label();
            btnChangePassword = new Button();
            btnClear = new Button();
            btnExport = new Button();
            btnOpenFolder = new Button();
            btnOpenFiles = new Button();
            pnlGrid = new Panel();
            lblDropHint = new Label();
            dgvResults = new DataGridView();
            colNumber = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colGroup = new DataGridViewTextBoxColumn();
            colDate = new DataGridViewTextBoxColumn();
            colDuration = new DataGridViewTextBoxColumn();
            colAttempt = new DataGridViewTextBoxColumn();
            colQ1 = new DataGridViewTextBoxColumn();
            colQ2 = new DataGridViewTextBoxColumn();
            colQ3 = new DataGridViewTextBoxColumn();
            colQ4 = new DataGridViewTextBoxColumn();
            colQ5 = new DataGridViewTextBoxColumn();
            colTotal = new DataGridViewTextBoxColumn();
            colGrade = new DataGridViewTextBoxColumn();
            colViolations = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colNote = new DataGridViewTextBoxColumn();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            openFileDialog = new OpenFileDialog();
            folderBrowserDialog = new FolderBrowserDialog();
            saveFileDialog = new SaveFileDialog();
            pnlTop.SuspendLayout();
            pnlToolbar.SuspendLayout();
            pnlPasswordHint.SuspendLayout();
            pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.BackColor = Color.FromArgb(31, 78, 121);
            pnlTop.Controls.Add(lblSubtitle);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1340, 70);
            pnlTop.TabIndex = 0;
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSubtitle.ForeColor = Color.FromArgb(214, 228, 242);
            lblSubtitle.Location = new Point(24, 44);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(470, 17);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Результати контрольних: перевірка справжності файлів і експорт у Excel";
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(22, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(193, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Режим викладача";
            //
            // pnlToolbar
            //
            pnlToolbar.BackColor = Color.FromArgb(232, 236, 241);
            pnlToolbar.Controls.Add(btnBankTools);
            pnlToolbar.Controls.Add(btnChangePassword);
            pnlToolbar.Controls.Add(btnClear);
            pnlToolbar.Controls.Add(btnExport);
            pnlToolbar.Controls.Add(btnOpenFolder);
            pnlToolbar.Controls.Add(btnOpenFiles);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 70);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1340, 56);
            pnlToolbar.TabIndex = 1;
            //
            // btnBankTools
            //
            btnBankTools.Location = new Point(728, 10);
            btnBankTools.Name = "btnBankTools";
            btnBankTools.Size = new Size(210, 36);
            btnBankTools.TabIndex = 5;
            btnBankTools.Text = "Банк питань і ключ…";
            btnBankTools.UseVisualStyleBackColor = true;
            btnBankTools.Click += BtnBankTools_Click;
            //
            // pnlPasswordHint
            //
            pnlPasswordHint.BackColor = Color.FromArgb(255, 243, 205);
            pnlPasswordHint.Controls.Add(btnCloseHint);
            pnlPasswordHint.Controls.Add(lnkChangePassword);
            pnlPasswordHint.Controls.Add(lblPasswordHint);
            pnlPasswordHint.Dock = DockStyle.Top;
            pnlPasswordHint.Location = new Point(0, 126);
            pnlPasswordHint.Name = "pnlPasswordHint";
            pnlPasswordHint.Size = new Size(1340, 38);
            pnlPasswordHint.TabIndex = 4;
            pnlPasswordHint.Visible = false;
            //
            // btnCloseHint
            //
            btnCloseHint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCloseHint.FlatAppearance.BorderSize = 0;
            btnCloseHint.FlatStyle = FlatStyle.Flat;
            btnCloseHint.Location = new Point(1296, 4);
            btnCloseHint.Name = "btnCloseHint";
            btnCloseHint.Size = new Size(30, 30);
            btnCloseHint.TabIndex = 2;
            btnCloseHint.Text = "✕";
            btnCloseHint.UseVisualStyleBackColor = true;
            btnCloseHint.Click += BtnCloseHint_Click;
            //
            // lnkChangePassword
            //
            lnkChangePassword.AutoSize = true;
            lnkChangePassword.Location = new Point(620, 10);
            lnkChangePassword.Name = "lnkChangePassword";
            lnkChangePassword.Size = new Size(104, 19);
            lnkChangePassword.TabIndex = 1;
            lnkChangePassword.TabStop = true;
            lnkChangePassword.Text = "Змінити пароль";
            lnkChangePassword.LinkClicked += LnkChangePassword_LinkClicked;
            //
            // lblPasswordHint
            //
            lblPasswordHint.AutoSize = true;
            lblPasswordHint.ForeColor = Color.FromArgb(102, 77, 3);
            lblPasswordHint.Location = new Point(20, 10);
            lblPasswordHint.Name = "lblPasswordHint";
            lblPasswordHint.Size = new Size(590, 19);
            lblPasswordHint.TabIndex = 0;
            lblPasswordHint.Text = "Ви увійшли з типовим паролем. Радимо змінити його, щоб режим викладача був лише у вас.";
            //
            // btnChangePassword
            //
            btnChangePassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChangePassword.Location = new Point(1156, 10);
            btnChangePassword.Name = "btnChangePassword";
            btnChangePassword.Size = new Size(164, 36);
            btnChangePassword.TabIndex = 4;
            btnChangePassword.Text = "Змінити пароль…";
            btnChangePassword.UseVisualStyleBackColor = true;
            btnChangePassword.Click += BtnChangePassword_Click;
            //
            // btnClear
            //
            btnClear.Location = new Point(588, 10);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(130, 36);
            btnClear.TabIndex = 3;
            btnClear.Text = "Очистити";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += BtnClear_Click;
            //
            // btnExport
            //
            btnExport.BackColor = Color.FromArgb(46, 125, 50);
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExport.ForeColor = Color.White;
            btnExport.Location = new Point(378, 10);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(200, 36);
            btnExport.TabIndex = 2;
            btnExport.Text = "Експорт у Excel…";
            btnExport.UseVisualStyleBackColor = false;
            btnExport.Click += BtnExport_Click;
            //
            // btnOpenFolder
            //
            btnOpenFolder.Location = new Point(198, 10);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(170, 36);
            btnOpenFolder.TabIndex = 1;
            btnOpenFolder.Text = "Відкрити теку…";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += BtnOpenFolder_Click;
            //
            // btnOpenFiles
            //
            btnOpenFiles.BackColor = Color.FromArgb(46, 117, 182);
            btnOpenFiles.FlatAppearance.BorderSize = 0;
            btnOpenFiles.FlatStyle = FlatStyle.Flat;
            btnOpenFiles.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenFiles.ForeColor = Color.White;
            btnOpenFiles.Location = new Point(18, 10);
            btnOpenFiles.Name = "btnOpenFiles";
            btnOpenFiles.Size = new Size(170, 36);
            btnOpenFiles.TabIndex = 0;
            btnOpenFiles.Text = "Відкрити файли…";
            btnOpenFiles.UseVisualStyleBackColor = false;
            btnOpenFiles.Click += BtnOpenFiles_Click;
            //
            // pnlGrid
            //
            pnlGrid.BackColor = Color.FromArgb(245, 247, 250);
            pnlGrid.Controls.Add(lblDropHint);
            pnlGrid.Controls.Add(dgvResults);
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Location = new Point(0, 126);
            pnlGrid.Name = "pnlGrid";
            pnlGrid.Padding = new Padding(14);
            pnlGrid.Size = new Size(1340, 492);
            pnlGrid.TabIndex = 2;
            //
            // lblDropHint
            //
            lblDropHint.BackColor = Color.White;
            lblDropHint.Font = new Font("Segoe UI", 11F);
            lblDropHint.ForeColor = Color.FromArgb(96, 108, 122);
            lblDropHint.Location = new Point(330, 200);
            lblDropHint.Name = "lblDropHint";
            lblDropHint.Size = new Size(520, 90);
            lblDropHint.TabIndex = 1;
            lblDropHint.Text = "Перетягніть сюди файли результатів учнів з Провідника\r\nабо натисніть «Відкрити файли…» чи «Відкрити теку…»";
            lblDropHint.TextAlign = ContentAlignment.MiddleCenter;
            //
            // dgvResults
            //
            dgvResults.AllowDrop = true;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.AllowUserToResizeRows = false;
            dgvResults.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvResults.BackgroundColor = Color.White;
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = Color.FromArgb(232, 242, 252);
            headerStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            headerStyle.ForeColor = Color.FromArgb(31, 78, 121);
            headerStyle.SelectionBackColor = Color.FromArgb(232, 242, 252);
            headerStyle.SelectionForeColor = Color.FromArgb(31, 78, 121);
            headerStyle.WrapMode = DataGridViewTriState.True;
            dgvResults.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvResults.ColumnHeadersHeight = 44;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Columns.AddRange(new DataGridViewColumn[] { colNumber, colName, colGroup, colDate, colDuration, colAttempt, colQ1, colQ2, colQ3, colQ4, colQ5, colTotal, colGrade, colViolations, colStatus, colNote });
            dgvResults.Dock = DockStyle.Fill;
            dgvResults.EnableHeadersVisualStyles = false;
            dgvResults.Location = new Point(14, 14);
            dgvResults.MultiSelect = false;
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.RowTemplate.Height = 30;
            dgvResults.RowTemplate.MinimumHeight = 30;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Size = new Size(1312, 464);
            dgvResults.TabIndex = 0;
            dgvResults.CellDoubleClick += DgvResults_CellDoubleClick;
            dgvResults.DragDrop += DgvResults_DragDrop;
            dgvResults.DragEnter += DgvResults_DragEnter;
            dgvResults.Resize += DgvResults_Resize;
            dgvResults.SortCompare += DgvResults_SortCompare;
            //
            // colNumber
            //
            colNumber.HeaderText = "№";
            colNumber.Name = "colNumber";
            colNumber.ReadOnly = true;
            colNumber.Width = 30;
            //
            // colName
            //
            nameStyle.WrapMode = DataGridViewTriState.True;
            colName.DefaultCellStyle = nameStyle;
            colName.HeaderText = "ПІБ";
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.Width = 140;
            //
            // colGroup
            //
            colGroup.HeaderText = "Група";
            colGroup.Name = "colGroup";
            colGroup.ReadOnly = true;
            colGroup.Width = 50;
            //
            // colDate
            //
            colDate.HeaderText = "Дата";
            colDate.Name = "colDate";
            colDate.ReadOnly = true;
            colDate.Width = 124;
            //
            // colDuration
            //
            colDuration.HeaderText = "Час (год:хв:с)";
            colDuration.Name = "colDuration";
            colDuration.ReadOnly = true;
            colDuration.Width = 74;
            //
            // colAttempt
            //
            colAttempt.HeaderText = "Спроба";
            colAttempt.ToolTipText = "Номер спроби цього учня серед відкритих файлів (за ПІБ і групою). Якщо лічильник на комп'ютері учня інший — це видно в «Примітці»";
            colAttempt.Name = "colAttempt";
            colAttempt.ReadOnly = true;
            colAttempt.Width = 56;
            //
            // colQ1
            //
            colQ1.HeaderText = "П1";
            colQ1.Name = "colQ1";
            colQ1.ReadOnly = true;
            colQ1.ToolTipText = "Питання 1: Кошик";
            colQ1.Width = 42;
            //
            // colQ2
            //
            colQ2.HeaderText = "П2";
            colQ2.Name = "colQ2";
            colQ2.ReadOnly = true;
            colQ2.ToolTipText = "Питання 2: Таблиця відповідностей";
            colQ2.Width = 42;
            //
            // colQ3
            //
            colQ3.HeaderText = "П3";
            colQ3.Name = "colQ3";
            colQ3.ReadOnly = true;
            colQ3.ToolTipText = "Питання 3: Номерки на категорії";
            colQ3.Width = 42;
            //
            // colQ4
            //
            colQ4.HeaderText = "П4";
            colQ4.Name = "colQ4";
            colQ4.ReadOnly = true;
            colQ4.ToolTipText = "Питання 4: Слова в пропуски";
            colQ4.Width = 42;
            //
            // colQ5
            //
            colQ5.HeaderText = "П5";
            colQ5.Name = "colQ5";
            colQ5.ReadOnly = true;
            colQ5.ToolTipText = "Питання 5: Порядок кроків";
            colQ5.Width = 42;
            //
            // colTotal
            //
            colTotal.HeaderText = "Бали (з 100)";
            colTotal.Name = "colTotal";
            colTotal.ReadOnly = true;
            colTotal.Width = 62;
            //
            // colGrade
            //
            colGrade.HeaderText = "Оцінка";
            colGrade.Name = "colGrade";
            colGrade.ReadOnly = true;
            colGrade.Width = 64;
            //
            // colViolations
            //
            violationStyle.WrapMode = DataGridViewTriState.True;
            colViolations.DefaultCellStyle = violationStyle;
            colViolations.HeaderText = "Порушення";
            colViolations.Name = "colViolations";
            colViolations.ReadOnly = true;
            colViolations.ToolTipText = "Режим доброчесності: виходи з вікна тесту, знімки екрана, дострокове завершення";
            colViolations.Width = 170;
            //
            // colStatus
            //
            colStatus.HeaderText = "Файл справжній";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.Width = 150;
            //
            // colNote
            //
            colNote.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            noteStyle.WrapMode = DataGridViewTriState.True;
            colNote.DefaultCellStyle = noteStyle;
            colNote.HeaderText = "Примітка";
            colNote.MinimumWidth = 140;
            colNote.Name = "colNote";
            colNote.ReadOnly = true;
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 618);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1340, 22);
            statusStrip.TabIndex = 3;
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(120, 17);
            lblStatus.Text = "Файлів не відкрито";
            //
            // openFileDialog
            //
            openFileDialog.Filter = "Файли результатів (*.json)|*.json|Усі файли (*.*)|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Відкрийте файли результатів учнів";
            //
            // folderBrowserDialog
            //
            folderBrowserDialog.Description = "Оберіть теку з файлами результатів учнів";
            folderBrowserDialog.UseDescriptionForTitle = true;
            //
            // saveFileDialog
            //
            saveFileDialog.DefaultExt = "xlsx";
            saveFileDialog.Filter = "Книга Excel (*.xlsx)|*.xlsx|Таблиця CSV (*.csv)|*.csv";
            saveFileDialog.Title = "Експорт результатів у Excel";
            //
            // TeacherForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1340, 640);
            Controls.Add(pnlGrid);
            Controls.Add(statusStrip);
            Controls.Add(pnlPasswordHint);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1000, 500);
            Name = "TeacherForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Режим викладача — Тестування з програмування на C#";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlToolbar.ResumeLayout(false);
            pnlPasswordHint.ResumeLayout(false);
            pnlPasswordHint.PerformLayout();
            pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlTop;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlToolbar;
        private Button btnOpenFiles;
        private Button btnOpenFolder;
        private Button btnExport;
        private Button btnClear;
        private Button btnChangePassword;
        private Button btnBankTools;
        private Panel pnlPasswordHint;
        private Label lblPasswordHint;
        private LinkLabel lnkChangePassword;
        private Button btnCloseHint;
        private Panel pnlGrid;
        private Label lblDropHint;
        private DataGridView dgvResults;
        private DataGridViewTextBoxColumn colNumber;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colGroup;
        private DataGridViewTextBoxColumn colDate;
        private DataGridViewTextBoxColumn colDuration;
        private DataGridViewTextBoxColumn colAttempt;
        private DataGridViewTextBoxColumn colQ1;
        private DataGridViewTextBoxColumn colQ2;
        private DataGridViewTextBoxColumn colQ3;
        private DataGridViewTextBoxColumn colQ4;
        private DataGridViewTextBoxColumn colQ5;
        private DataGridViewTextBoxColumn colTotal;
        private DataGridViewTextBoxColumn colGrade;
        private DataGridViewTextBoxColumn colViolations;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colNote;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog folderBrowserDialog;
        private SaveFileDialog saveFileDialog;
    }
}

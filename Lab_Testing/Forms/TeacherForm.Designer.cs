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
            pnlTop = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            pnlToolbar = new Panel();
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
            colStatus = new DataGridViewTextBoxColumn();
            colFile = new DataGridViewTextBoxColumn();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            openFileDialog = new OpenFileDialog();
            folderBrowserDialog = new FolderBrowserDialog();
            saveFileDialog = new SaveFileDialog();
            pnlTop.SuspendLayout();
            pnlToolbar.SuspendLayout();
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
            pnlTop.Size = new Size(1180, 70);
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
            pnlToolbar.Controls.Add(btnChangePassword);
            pnlToolbar.Controls.Add(btnClear);
            pnlToolbar.Controls.Add(btnExport);
            pnlToolbar.Controls.Add(btnOpenFolder);
            pnlToolbar.Controls.Add(btnOpenFiles);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 70);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1180, 56);
            pnlToolbar.TabIndex = 1;
            //
            // btnChangePassword
            //
            btnChangePassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChangePassword.Location = new Point(996, 10);
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
            btnExport.Text = "Експорт у Excel (CSV)…";
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
            pnlGrid.Size = new Size(1180, 492);
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
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvResults.Columns.AddRange(new DataGridViewColumn[] { colNumber, colName, colGroup, colDate, colDuration, colAttempt, colQ1, colQ2, colQ3, colQ4, colQ5, colTotal, colGrade, colStatus, colFile });
            dgvResults.Dock = DockStyle.Fill;
            dgvResults.EnableHeadersVisualStyles = false;
            dgvResults.Location = new Point(14, 14);
            dgvResults.MultiSelect = false;
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.RowTemplate.Height = 30;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Size = new Size(1152, 464);
            dgvResults.TabIndex = 0;
            dgvResults.CellDoubleClick += DgvResults_CellDoubleClick;
            dgvResults.DragDrop += DgvResults_DragDrop;
            dgvResults.DragEnter += DgvResults_DragEnter;
            dgvResults.Resize += DgvResults_Resize;
            //
            // colNumber
            //
            colNumber.FillWeight = 4F;
            colNumber.HeaderText = "№";
            colNumber.Name = "colNumber";
            colNumber.ReadOnly = true;
            //
            // colName
            //
            colName.FillWeight = 20F;
            colName.HeaderText = "ПІБ";
            colName.Name = "colName";
            colName.ReadOnly = true;
            //
            // colGroup
            //
            colGroup.FillWeight = 7F;
            colGroup.HeaderText = "Група";
            colGroup.Name = "colGroup";
            colGroup.ReadOnly = true;
            //
            // colDate
            //
            colDate.FillWeight = 12F;
            colDate.HeaderText = "Дата";
            colDate.Name = "colDate";
            colDate.ReadOnly = true;
            //
            // colDuration
            //
            colDuration.FillWeight = 8F;
            colDuration.HeaderText = "Час (год:хв:с)";
            colDuration.Name = "colDuration";
            colDuration.ReadOnly = true;
            //
            // colAttempt
            //
            colAttempt.FillWeight = 6F;
            colAttempt.HeaderText = "Спроба";
            colAttempt.Name = "colAttempt";
            colAttempt.ReadOnly = true;
            //
            // colQ1
            //
            colQ1.FillWeight = 5F;
            colQ1.HeaderText = "П1";
            colQ1.Name = "colQ1";
            colQ1.ReadOnly = true;
            colQ1.ToolTipText = "Питання 1: Кошик";
            //
            // colQ2
            //
            colQ2.FillWeight = 5F;
            colQ2.HeaderText = "П2";
            colQ2.Name = "colQ2";
            colQ2.ReadOnly = true;
            colQ2.ToolTipText = "Питання 2: Таблиця відповідностей";
            //
            // colQ3
            //
            colQ3.FillWeight = 5F;
            colQ3.HeaderText = "П3";
            colQ3.Name = "colQ3";
            colQ3.ReadOnly = true;
            colQ3.ToolTipText = "Питання 3: Номерки на категорії";
            //
            // colQ4
            //
            colQ4.FillWeight = 5F;
            colQ4.HeaderText = "П4";
            colQ4.Name = "colQ4";
            colQ4.ReadOnly = true;
            colQ4.ToolTipText = "Питання 4: Слова в пропуски";
            //
            // colQ5
            //
            colQ5.FillWeight = 5F;
            colQ5.HeaderText = "П5";
            colQ5.Name = "colQ5";
            colQ5.ReadOnly = true;
            colQ5.ToolTipText = "Питання 5: Порядок кроків";
            //
            // colTotal
            //
            colTotal.FillWeight = 7F;
            colTotal.HeaderText = "Бали (з 100)";
            colTotal.Name = "colTotal";
            colTotal.ReadOnly = true;
            //
            // colGrade
            //
            colGrade.FillWeight = 7F;
            colGrade.HeaderText = "Оцінка";
            colGrade.Name = "colGrade";
            colGrade.ReadOnly = true;
            //
            // colStatus
            //
            colStatus.FillWeight = 13F;
            colStatus.HeaderText = "Файл справжній";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            //
            // colFile
            //
            colFile.FillWeight = 16F;
            colFile.HeaderText = "Файл";
            colFile.Name = "colFile";
            colFile.ReadOnly = true;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 618);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1180, 22);
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
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.Filter = "Таблиця для Excel (*.csv)|*.csv";
            saveFileDialog.Title = "Експорт результатів у Excel";
            //
            // TeacherForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1180, 640);
            Controls.Add(pnlGrid);
            Controls.Add(statusStrip);
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
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colFile;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog folderBrowserDialog;
        private SaveFileDialog saveFileDialog;
    }
}

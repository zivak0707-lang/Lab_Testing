namespace Lab_Testing
{
    partial class StartForm
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
            pnlHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            grpStudent = new GroupBox();
            lblNameHint = new Label();
            txtGroup = new TextBox();
            lblGroup = new Label();
            txtName = new TextBox();
            lblName = new Label();
            lblMode = new Label();
            pnlTraining = new Panel();
            lblTrainingDesc = new Label();
            rbTraining = new RadioButton();
            pnlControl = new Panel();
            lblControlDesc = new Label();
            rbControl = new RadioButton();
            btnStart = new Button();
            lblBankInfo = new Label();
            pnlFooter = new Panel();
            lnkAbout = new LinkLabel();
            lnkTeacher = new LinkLabel();
            lblAuthor = new Label();
            pnlHeader.SuspendLayout();
            grpStudent.SuspendLayout();
            pnlTraining.SuspendLayout();
            pnlControl.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(31, 78, 121);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(860, 100);
            pnlHeader.TabIndex = 0;
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 10.5F);
            lblSubtitle.ForeColor = Color.FromArgb(214, 228, 242);
            lblSubtitle.Location = new Point(30, 62);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(590, 19);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Основи C#: типи даних · оператори · значення та їх типи · break і continue · алгоритми";
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(26, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(469, 37);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Тестування з програмування на C#";
            //
            // grpStudent
            //
            grpStudent.Controls.Add(lblNameHint);
            grpStudent.Controls.Add(txtGroup);
            grpStudent.Controls.Add(lblGroup);
            grpStudent.Controls.Add(txtName);
            grpStudent.Controls.Add(lblName);
            grpStudent.Location = new Point(28, 118);
            grpStudent.Name = "grpStudent";
            grpStudent.Size = new Size(804, 128);
            grpStudent.TabIndex = 1;
            grpStudent.TabStop = false;
            grpStudent.Text = "Дані учня";
            //
            // lblNameHint
            //
            lblNameHint.AutoSize = true;
            lblNameHint.Font = new Font("Segoe UI", 8.5F);
            lblNameHint.ForeColor = Color.FromArgb(96, 108, 122);
            lblNameHint.Location = new Point(232, 58);
            lblNameHint.Name = "lblNameHint";
            lblNameHint.Size = new Size(208, 15);
            lblNameHint.TabIndex = 4;
            lblNameHint.Text = "Наприклад: Іваненко Олена Петрівна";
            //
            // txtGroup
            //
            txtGroup.Location = new Point(232, 84);
            txtGroup.MaxLength = 20;
            txtGroup.Name = "txtGroup";
            txtGroup.Size = new Size(160, 25);
            txtGroup.TabIndex = 3;
            //
            // lblGroup
            //
            lblGroup.AutoSize = true;
            lblGroup.Location = new Point(18, 87);
            lblGroup.Name = "lblGroup";
            lblGroup.Size = new Size(49, 19);
            lblGroup.TabIndex = 2;
            lblGroup.Text = "Група:";
            //
            // txtName
            //
            txtName.Location = new Point(232, 30);
            txtName.MaxLength = 80;
            txtName.Name = "txtName";
            txtName.Size = new Size(552, 25);
            txtName.TabIndex = 1;
            //
            // lblName
            //
            lblName.AutoSize = true;
            lblName.Location = new Point(18, 33);
            lblName.Name = "lblName";
            lblName.Size = new Size(190, 19);
            lblName.TabIndex = 0;
            lblName.Text = "Прізвище, ім'я, по батькові:";
            //
            // lblMode
            //
            lblMode.AutoSize = true;
            lblMode.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMode.Location = new Point(26, 262);
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(206, 20);
            lblMode.TabIndex = 2;
            lblMode.Text = "Оберіть режим тестування:";
            //
            // pnlTraining
            //
            pnlTraining.BackColor = Color.White;
            pnlTraining.Controls.Add(lblTrainingDesc);
            pnlTraining.Controls.Add(rbTraining);
            pnlTraining.Cursor = Cursors.Hand;
            pnlTraining.Location = new Point(28, 294);
            pnlTraining.Name = "pnlTraining";
            pnlTraining.Size = new Size(394, 196);
            pnlTraining.TabIndex = 3;
            pnlTraining.Click += ModeCard_Click;
            pnlTraining.Paint += ModeCard_Paint;
            //
            // lblTrainingDesc
            //
            lblTrainingDesc.ForeColor = Color.FromArgb(60, 72, 86);
            lblTrainingDesc.Location = new Point(38, 52);
            lblTrainingDesc.Name = "lblTrainingDesc";
            lblTrainingDesc.Size = new Size(340, 132);
            lblTrainingDesc.TabIndex = 1;
            lblTrainingDesc.Text = "• коротка теорія до кожного питання\r\n• кнопка «Перевірити» з підсвіткою відповідей\r\n• пояснення після перевірки\r\n• необмежена кількість спроб\r\n• результат не зберігається у файл";
            lblTrainingDesc.Click += ModeCard_Click;
            //
            // rbTraining
            //
            rbTraining.AutoSize = true;
            rbTraining.Checked = true;
            rbTraining.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            rbTraining.Location = new Point(16, 14);
            rbTraining.Name = "rbTraining";
            rbTraining.Size = new Size(113, 29);
            rbTraining.TabIndex = 0;
            rbTraining.TabStop = true;
            rbTraining.Text = "Навчання";
            rbTraining.UseVisualStyleBackColor = true;
            rbTraining.CheckedChanged += RbMode_CheckedChanged;
            //
            // pnlControl
            //
            pnlControl.BackColor = Color.White;
            pnlControl.Controls.Add(lblControlDesc);
            pnlControl.Controls.Add(rbControl);
            pnlControl.Cursor = Cursors.Hand;
            pnlControl.Location = new Point(438, 294);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(394, 196);
            pnlControl.TabIndex = 4;
            pnlControl.Click += ModeCard_Click;
            pnlControl.Paint += ModeCard_Paint;
            //
            // lblControlDesc
            //
            lblControlDesc.ForeColor = Color.FromArgb(60, 72, 86);
            lblControlDesc.Location = new Point(38, 52);
            lblControlDesc.Name = "lblControlDesc";
            lblControlDesc.Size = new Size(340, 132);
            lblControlDesc.TabIndex = 1;
            lblControlDesc.Text = "• без теорії, підказок і перевірки\r\n• обмеження часу: 15 хв\r\n• одна спроба\r\n• результат зберігається у файл,\r\n   який треба здати викладачеві";
            lblControlDesc.Click += ModeCard_Click;
            //
            // rbControl
            //
            rbControl.AutoSize = true;
            rbControl.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            rbControl.Location = new Point(16, 14);
            rbControl.Name = "rbControl";
            rbControl.Size = new Size(131, 29);
            rbControl.TabIndex = 0;
            rbControl.Text = "Контрольна";
            rbControl.UseVisualStyleBackColor = true;
            rbControl.CheckedChanged += RbMode_CheckedChanged;
            //
            // btnStart
            //
            btnStart.BackColor = Color.FromArgb(46, 117, 182);
            btnStart.Cursor = Cursors.Hand;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(28, 506);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(804, 50);
            btnStart.TabIndex = 5;
            btnStart.Text = "Почати тест";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += BtnStart_Click;
            //
            // lblBankInfo
            //
            lblBankInfo.AutoSize = true;
            lblBankInfo.Font = new Font("Segoe UI", 9F);
            lblBankInfo.ForeColor = Color.FromArgb(96, 108, 122);
            lblBankInfo.Location = new Point(26, 566);
            lblBankInfo.Name = "lblBankInfo";
            lblBankInfo.Size = new Size(83, 15);
            lblBankInfo.TabIndex = 6;
            lblBankInfo.Text = "Банк питань: —";
            //
            // pnlFooter
            //
            pnlFooter.BackColor = Color.FromArgb(232, 236, 241);
            pnlFooter.Controls.Add(lnkAbout);
            pnlFooter.Controls.Add(lnkTeacher);
            pnlFooter.Controls.Add(lblAuthor);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 598);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(860, 42);
            pnlFooter.TabIndex = 7;
            //
            // lnkAbout
            //
            lnkAbout.AutoSize = true;
            lnkAbout.Font = new Font("Segoe UI", 9.5F);
            lnkAbout.LinkColor = Color.FromArgb(46, 117, 182);
            lnkAbout.Location = new Point(738, 12);
            lnkAbout.Name = "lnkAbout";
            lnkAbout.Size = new Size(94, 17);
            lnkAbout.TabIndex = 2;
            lnkAbout.TabStop = true;
            lnkAbout.Text = "Про програму";
            lnkAbout.LinkClicked += LnkAbout_LinkClicked;
            //
            // lnkTeacher
            //
            lnkTeacher.AutoSize = true;
            lnkTeacher.Font = new Font("Segoe UI", 9.5F);
            lnkTeacher.LinkColor = Color.FromArgb(46, 117, 182);
            lnkTeacher.Location = new Point(598, 12);
            lnkTeacher.Name = "lnkTeacher";
            lnkTeacher.Size = new Size(112, 17);
            lnkTeacher.TabIndex = 1;
            lnkTeacher.TabStop = true;
            lnkTeacher.Text = "Режим викладача";
            lnkTeacher.LinkClicked += LnkTeacher_LinkClicked;
            //
            // lblAuthor
            //
            lblAuthor.AutoSize = true;
            lblAuthor.Font = new Font("Segoe UI", 9F);
            lblAuthor.ForeColor = Color.FromArgb(60, 72, 86);
            lblAuthor.Location = new Point(26, 13);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(250, 15);
            lblAuthor.TabIndex = 0;
            lblAuthor.Text = "Автор: Сергій Зівак, група 471 · версія 1.0.0";
            //
            // StartForm
            //
            AcceptButton = btnStart;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(860, 640);
            Controls.Add(pnlFooter);
            Controls.Add(lblBankInfo);
            Controls.Add(btnStart);
            Controls.Add(pnlControl);
            Controls.Add(pnlTraining);
            Controls.Add(lblMode);
            Controls.Add(grpStudent);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "StartForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Тестування з програмування на C#";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            grpStudent.ResumeLayout(false);
            grpStudent.PerformLayout();
            pnlTraining.ResumeLayout(false);
            pnlTraining.PerformLayout();
            pnlControl.ResumeLayout(false);
            pnlControl.PerformLayout();
            pnlFooter.ResumeLayout(false);
            pnlFooter.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private GroupBox grpStudent;
        private Label lblName;
        private TextBox txtName;
        private Label lblNameHint;
        private Label lblGroup;
        private TextBox txtGroup;
        private Label lblMode;
        private Panel pnlTraining;
        private RadioButton rbTraining;
        private Label lblTrainingDesc;
        private Panel pnlControl;
        private RadioButton rbControl;
        private Label lblControlDesc;
        private Button btnStart;
        private Label lblBankInfo;
        private Panel pnlFooter;
        private Label lblAuthor;
        private LinkLabel lnkTeacher;
        private LinkLabel lnkAbout;
    }
}

namespace Lab_Testing
{
    partial class ResultForm
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
            pnlHeader = new Panel();
            pnlGrade = new Panel();
            lblLevel = new Label();
            lblGradeCaption = new Label();
            lblGrade = new Label();
            lblDetails = new Label();
            lblStudent = new Label();
            lblTitle = new Label();
            pnlSummary = new Panel();
            lblScore = new Label();
            dgvQuestions = new DataGridView();
            colNumber = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            colTitle = new DataGridViewTextBoxColumn();
            colSummary = new DataGridViewTextBoxColumn();
            colScore = new DataGridViewTextBoxColumn();
            lblReviewTitle = new Label();
            pnlReview = new Panel();
            rtbReview = new RichTextBox();
            pnlBottom = new Panel();
            lblFileInfo = new Label();
            btnClose = new Button();
            pnlHeader.SuspendLayout();
            pnlGrade.SuspendLayout();
            pnlSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvQuestions).BeginInit();
            pnlReview.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(31, 78, 121);
            pnlHeader.Controls.Add(pnlGrade);
            pnlHeader.Controls.Add(lblDetails);
            pnlHeader.Controls.Add(lblStudent);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1000, 116);
            pnlHeader.TabIndex = 0;
            //
            // pnlGrade
            //
            pnlGrade.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlGrade.BackColor = Color.White;
            pnlGrade.Controls.Add(lblLevel);
            pnlGrade.Controls.Add(lblGradeCaption);
            pnlGrade.Controls.Add(lblGrade);
            pnlGrade.Location = new Point(796, 12);
            pnlGrade.Name = "pnlGrade";
            pnlGrade.Size = new Size(184, 92);
            pnlGrade.TabIndex = 3;
            //
            // lblLevel
            //
            lblLevel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLevel.ForeColor = Color.FromArgb(46, 117, 182);
            lblLevel.Location = new Point(0, 70);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(184, 18);
            lblLevel.TabIndex = 2;
            lblLevel.Text = "достатній рівень";
            lblLevel.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblGradeCaption
            //
            lblGradeCaption.Font = new Font("Segoe UI", 8.5F);
            lblGradeCaption.ForeColor = Color.FromArgb(96, 108, 122);
            lblGradeCaption.Location = new Point(0, 52);
            lblGradeCaption.Name = "lblGradeCaption";
            lblGradeCaption.Size = new Size(184, 18);
            lblGradeCaption.TabIndex = 1;
            lblGradeCaption.Text = "оцінка за 12-бальною шкалою";
            lblGradeCaption.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblGrade
            //
            lblGrade.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblGrade.ForeColor = Color.FromArgb(31, 78, 121);
            lblGrade.Location = new Point(0, 2);
            lblGrade.Name = "lblGrade";
            lblGrade.Size = new Size(184, 50);
            lblGrade.TabIndex = 0;
            lblGrade.Text = "12";
            lblGrade.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblDetails
            //
            lblDetails.AutoSize = true;
            lblDetails.Font = new Font("Segoe UI", 9.5F);
            lblDetails.ForeColor = Color.FromArgb(214, 228, 242);
            lblDetails.Location = new Point(26, 84);
            lblDetails.Name = "lblDetails";
            lblDetails.Size = new Size(260, 17);
            lblDetails.TabIndex = 2;
            lblDetails.Text = "дата · час проходження";
            //
            // lblStudent
            //
            lblStudent.AutoSize = true;
            lblStudent.Font = new Font("Segoe UI", 11F);
            lblStudent.ForeColor = Color.White;
            lblStudent.Location = new Point(26, 56);
            lblStudent.Name = "lblStudent";
            lblStudent.Size = new Size(240, 20);
            lblStudent.TabIndex = 1;
            lblStudent.Text = "ПІБ · група · режим";
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(24, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(210, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Результат тесту";
            //
            // pnlSummary
            //
            pnlSummary.BackColor = Color.White;
            pnlSummary.Controls.Add(lblScore);
            pnlSummary.Dock = DockStyle.Top;
            pnlSummary.Location = new Point(0, 116);
            pnlSummary.Name = "pnlSummary";
            pnlSummary.Size = new Size(1000, 52);
            pnlSummary.TabIndex = 1;
            //
            // lblScore
            //
            lblScore.AutoSize = true;
            lblScore.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblScore.ForeColor = Color.FromArgb(25, 35, 48);
            lblScore.Location = new Point(24, 13);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(260, 25);
            lblScore.TabIndex = 0;
            lblScore.Text = "0 з 100 балів · 0 %";
            //
            // dgvQuestions
            //
            dgvQuestions.AllowUserToAddRows = false;
            dgvQuestions.AllowUserToDeleteRows = false;
            dgvQuestions.AllowUserToResizeRows = false;
            dgvQuestions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvQuestions.BackgroundColor = Color.White;
            dgvQuestions.BorderStyle = BorderStyle.None;
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = Color.FromArgb(232, 242, 252);
            headerStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            headerStyle.ForeColor = Color.FromArgb(31, 78, 121);
            headerStyle.SelectionBackColor = Color.FromArgb(232, 242, 252);
            headerStyle.SelectionForeColor = Color.FromArgb(31, 78, 121);
            dgvQuestions.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvQuestions.ColumnHeadersHeight = 34;
            dgvQuestions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvQuestions.Columns.AddRange(new DataGridViewColumn[] { colNumber, colType, colTitle, colSummary, colScore });
            dgvQuestions.Dock = DockStyle.Top;
            dgvQuestions.EnableHeadersVisualStyles = false;
            dgvQuestions.Location = new Point(0, 168);
            dgvQuestions.MultiSelect = false;
            dgvQuestions.Name = "dgvQuestions";
            dgvQuestions.ReadOnly = true;
            dgvQuestions.RowHeadersVisible = false;
            dgvQuestions.RowTemplate.Height = 30;
            dgvQuestions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvQuestions.Size = new Size(1000, 186);
            dgvQuestions.TabIndex = 2;
            //
            // colNumber
            //
            colNumber.FillWeight = 8F;
            colNumber.HeaderText = "№";
            colNumber.Name = "colNumber";
            colNumber.ReadOnly = true;
            //
            // colType
            //
            colType.FillWeight = 30F;
            colType.HeaderText = "Тип завдання";
            colType.Name = "colType";
            colType.ReadOnly = true;
            //
            // colTitle
            //
            colTitle.FillWeight = 38F;
            colTitle.HeaderText = "Питання";
            colTitle.Name = "colTitle";
            colTitle.ReadOnly = true;
            //
            // colSummary
            //
            colSummary.FillWeight = 38F;
            colSummary.HeaderText = "Результат";
            colSummary.Name = "colSummary";
            colSummary.ReadOnly = true;
            //
            // colScore
            //
            colScore.FillWeight = 16F;
            colScore.HeaderText = "Бали";
            colScore.Name = "colScore";
            colScore.ReadOnly = true;
            //
            // lblReviewTitle
            //
            lblReviewTitle.Dock = DockStyle.Top;
            lblReviewTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblReviewTitle.ForeColor = Color.FromArgb(31, 78, 121);
            lblReviewTitle.Location = new Point(0, 354);
            lblReviewTitle.Name = "lblReviewTitle";
            lblReviewTitle.Padding = new Padding(22, 0, 0, 0);
            lblReviewTitle.Size = new Size(1000, 36);
            lblReviewTitle.TabIndex = 3;
            lblReviewTitle.Text = "Розбір помилок";
            lblReviewTitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // pnlReview
            //
            pnlReview.BackColor = Color.White;
            pnlReview.Controls.Add(rtbReview);
            pnlReview.Dock = DockStyle.Fill;
            pnlReview.Location = new Point(0, 390);
            pnlReview.Name = "pnlReview";
            pnlReview.Padding = new Padding(24, 8, 12, 8);
            pnlReview.Size = new Size(1000, 230);
            pnlReview.TabIndex = 4;
            //
            // rtbReview
            //
            rtbReview.BackColor = Color.White;
            rtbReview.BorderStyle = BorderStyle.None;
            rtbReview.Dock = DockStyle.Fill;
            rtbReview.Location = new Point(24, 8);
            rtbReview.Name = "rtbReview";
            rtbReview.ReadOnly = true;
            rtbReview.Size = new Size(964, 214);
            rtbReview.TabIndex = 0;
            rtbReview.Text = "";
            //
            // pnlBottom
            //
            pnlBottom.BackColor = Color.FromArgb(232, 236, 241);
            pnlBottom.Controls.Add(lblFileInfo);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 620);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1000, 60);
            pnlBottom.TabIndex = 5;
            //
            // lblFileInfo
            //
            lblFileInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileInfo.ForeColor = Color.FromArgb(60, 72, 86);
            lblFileInfo.Location = new Point(24, 12);
            lblFileInfo.Name = "lblFileInfo";
            lblFileInfo.Size = new Size(790, 36);
            lblFileInfo.TabIndex = 0;
            lblFileInfo.Text = "Режим «Навчання»: результат не зберігається у файл.";
            lblFileInfo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(46, 117, 182);
            btnClose.Cursor = Cursors.Hand;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(830, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(150, 36);
            btnClose.TabIndex = 1;
            btnClose.Text = "Закрити";
            btnClose.UseVisualStyleBackColor = false;
            //
            // ResultForm
            //
            AcceptButton = btnClose;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            CancelButton = btnClose;
            ClientSize = new Size(1000, 680);
            Controls.Add(pnlReview);
            Controls.Add(lblReviewTitle);
            Controls.Add(dgvQuestions);
            Controls.Add(pnlSummary);
            Controls.Add(pnlBottom);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(900, 600);
            Name = "ResultForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Результат тесту";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlGrade.ResumeLayout(false);
            pnlSummary.ResumeLayout(false);
            pnlSummary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvQuestions).EndInit();
            pnlReview.ResumeLayout(false);
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblStudent;
        private Label lblDetails;
        private Panel pnlGrade;
        private Label lblGrade;
        private Label lblGradeCaption;
        private Label lblLevel;
        private Panel pnlSummary;
        private Label lblScore;
        private DataGridView dgvQuestions;
        private DataGridViewTextBoxColumn colNumber;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colTitle;
        private DataGridViewTextBoxColumn colSummary;
        private DataGridViewTextBoxColumn colScore;
        private Label lblReviewTitle;
        private Panel pnlReview;
        private RichTextBox rtbReview;
        private Panel pnlBottom;
        private Label lblFileInfo;
        private Button btnClose;
    }
}

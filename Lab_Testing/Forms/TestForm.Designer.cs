namespace Lab_Testing
{
    partial class TestForm
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
            lblTimer = new Label();
            flpSteps = new FlowLayoutPanel();
            lblStudent = new Label();
            pnlQuestionHeader = new Panel();
            lblTask = new Label();
            lblQuestionInfo = new Label();
            pnlContent = new Panel();
            pnlTheory = new Panel();
            tlpTheory = new TableLayoutPanel();
            lblTheoryTitle = new Label();
            lblTheory = new Label();
            lblCheckTitle = new Label();
            lblCheckSummary = new Label();
            lblExplanation = new Label();
            pnlBottom = new Panel();
            btnReset = new Button();
            btnCheck = new Button();
            btnFinish = new Button();
            btnNext = new Button();
            btnPrev = new Button();
            pnlTop.SuspendLayout();
            pnlQuestionHeader.SuspendLayout();
            pnlTheory.SuspendLayout();
            tlpTheory.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.BackColor = Color.FromArgb(31, 78, 121);
            pnlTop.Controls.Add(lblTimer);
            pnlTop.Controls.Add(flpSteps);
            pnlTop.Controls.Add(lblStudent);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1100, 56);
            pnlTop.TabIndex = 0;
            //
            // lblTimer
            //
            lblTimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTimer.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTimer.ForeColor = Color.White;
            lblTimer.Location = new Point(688, 12);
            lblTimer.Name = "lblTimer";
            lblTimer.Size = new Size(160, 32);
            lblTimer.TabIndex = 2;
            lblTimer.Text = "15:00";
            lblTimer.TextAlign = ContentAlignment.MiddleRight;
            lblTimer.Visible = false;
            //
            // flpSteps
            //
            flpSteps.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flpSteps.Location = new Point(864, 9);
            flpSteps.Name = "flpSteps";
            flpSteps.Size = new Size(222, 40);
            flpSteps.TabIndex = 1;
            flpSteps.WrapContents = false;
            //
            // lblStudent
            //
            lblStudent.AutoSize = true;
            lblStudent.Font = new Font("Segoe UI", 10.5F);
            lblStudent.ForeColor = Color.White;
            lblStudent.Location = new Point(20, 18);
            lblStudent.Name = "lblStudent";
            lblStudent.Size = new Size(240, 19);
            lblStudent.TabIndex = 0;
            lblStudent.Text = "ПІБ  ·  група  ·  режим";
            //
            // pnlQuestionHeader
            //
            pnlQuestionHeader.BackColor = Color.White;
            pnlQuestionHeader.Controls.Add(lblTask);
            pnlQuestionHeader.Controls.Add(lblQuestionInfo);
            pnlQuestionHeader.Dock = DockStyle.Top;
            pnlQuestionHeader.Location = new Point(0, 56);
            pnlQuestionHeader.Name = "pnlQuestionHeader";
            pnlQuestionHeader.Padding = new Padding(20, 10, 20, 6);
            pnlQuestionHeader.Size = new Size(1100, 88);
            pnlQuestionHeader.TabIndex = 1;
            //
            // lblTask
            //
            lblTask.Dock = DockStyle.Fill;
            lblTask.Font = new Font("Segoe UI", 12F);
            lblTask.ForeColor = Color.FromArgb(25, 35, 48);
            lblTask.Location = new Point(20, 34);
            lblTask.Name = "lblTask";
            lblTask.Size = new Size(1060, 48);
            lblTask.TabIndex = 1;
            lblTask.Text = "Текст завдання";
            lblTask.UseMnemonic = false;
            //
            // lblQuestionInfo
            //
            lblQuestionInfo.Dock = DockStyle.Top;
            lblQuestionInfo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblQuestionInfo.ForeColor = Color.FromArgb(46, 117, 182);
            lblQuestionInfo.Location = new Point(20, 10);
            lblQuestionInfo.Name = "lblQuestionInfo";
            lblQuestionInfo.Size = new Size(1060, 24);
            lblQuestionInfo.TabIndex = 0;
            lblQuestionInfo.Text = "Питання 1 з 5  ·  тип  ·  тема";
            lblQuestionInfo.UseMnemonic = false;
            //
            // pnlContent
            //
            pnlContent.BackColor = Color.FromArgb(245, 247, 250);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 144);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(14, 8, 14, 8);
            pnlContent.Size = new Size(810, 472);
            pnlContent.TabIndex = 2;
            //
            // pnlTheory
            //
            pnlTheory.AutoScroll = true;
            pnlTheory.BackColor = Color.FromArgb(255, 249, 232);
            pnlTheory.Controls.Add(tlpTheory);
            pnlTheory.Dock = DockStyle.Right;
            pnlTheory.Location = new Point(810, 144);
            pnlTheory.Name = "pnlTheory";
            pnlTheory.Padding = new Padding(14, 12, 14, 12);
            pnlTheory.Size = new Size(290, 472);
            pnlTheory.TabIndex = 3;
            //
            // tlpTheory
            //
            tlpTheory.AutoSize = true;
            tlpTheory.ColumnCount = 1;
            tlpTheory.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpTheory.Controls.Add(lblTheoryTitle, 0, 0);
            tlpTheory.Controls.Add(lblTheory, 0, 1);
            tlpTheory.Controls.Add(lblCheckTitle, 0, 2);
            tlpTheory.Controls.Add(lblCheckSummary, 0, 3);
            tlpTheory.Controls.Add(lblExplanation, 0, 4);
            tlpTheory.Dock = DockStyle.Top;
            tlpTheory.Location = new Point(14, 12);
            tlpTheory.Name = "tlpTheory";
            tlpTheory.RowCount = 5;
            tlpTheory.RowStyles.Add(new RowStyle());
            tlpTheory.RowStyles.Add(new RowStyle());
            tlpTheory.RowStyles.Add(new RowStyle());
            tlpTheory.RowStyles.Add(new RowStyle());
            tlpTheory.RowStyles.Add(new RowStyle());
            tlpTheory.Size = new Size(262, 180);
            tlpTheory.TabIndex = 0;
            //
            // lblTheoryTitle
            //
            lblTheoryTitle.AutoSize = true;
            lblTheoryTitle.Dock = DockStyle.Fill;
            lblTheoryTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblTheoryTitle.ForeColor = Color.FromArgb(31, 78, 121);
            lblTheoryTitle.Location = new Point(3, 0);
            lblTheoryTitle.Margin = new Padding(3, 0, 3, 6);
            lblTheoryTitle.Name = "lblTheoryTitle";
            lblTheoryTitle.Size = new Size(256, 19);
            lblTheoryTitle.TabIndex = 0;
            lblTheoryTitle.Text = "Теорія";
            //
            // lblTheory
            //
            lblTheory.AutoSize = true;
            lblTheory.Dock = DockStyle.Fill;
            lblTheory.Location = new Point(3, 25);
            lblTheory.Margin = new Padding(3, 0, 3, 14);
            lblTheory.Name = "lblTheory";
            lblTheory.Size = new Size(256, 17);
            lblTheory.TabIndex = 1;
            lblTheory.Text = "Текст теорії";
            lblTheory.UseMnemonic = false;
            //
            // lblCheckTitle
            //
            lblCheckTitle.AutoSize = true;
            lblCheckTitle.Dock = DockStyle.Fill;
            lblCheckTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblCheckTitle.ForeColor = Color.FromArgb(31, 78, 121);
            lblCheckTitle.Location = new Point(3, 56);
            lblCheckTitle.Margin = new Padding(3, 0, 3, 6);
            lblCheckTitle.Name = "lblCheckTitle";
            lblCheckTitle.Size = new Size(256, 19);
            lblCheckTitle.TabIndex = 2;
            lblCheckTitle.Text = "Результат перевірки";
            lblCheckTitle.Visible = false;
            //
            // lblCheckSummary
            //
            lblCheckSummary.AutoSize = true;
            lblCheckSummary.Dock = DockStyle.Fill;
            lblCheckSummary.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCheckSummary.Location = new Point(3, 81);
            lblCheckSummary.Margin = new Padding(3, 0, 3, 8);
            lblCheckSummary.Name = "lblCheckSummary";
            lblCheckSummary.Size = new Size(256, 19);
            lblCheckSummary.TabIndex = 3;
            lblCheckSummary.Text = "Підсумок перевірки";
            lblCheckSummary.UseMnemonic = false;
            lblCheckSummary.Visible = false;
            //
            // lblExplanation
            //
            lblExplanation.AutoSize = true;
            lblExplanation.Dock = DockStyle.Fill;
            lblExplanation.Location = new Point(3, 108);
            lblExplanation.Margin = new Padding(3, 0, 3, 0);
            lblExplanation.Name = "lblExplanation";
            lblExplanation.Size = new Size(256, 17);
            lblExplanation.TabIndex = 4;
            lblExplanation.Text = "Пояснення";
            lblExplanation.UseMnemonic = false;
            lblExplanation.Visible = false;
            //
            // pnlBottom
            //
            pnlBottom.BackColor = Color.FromArgb(232, 236, 241);
            pnlBottom.Controls.Add(btnReset);
            pnlBottom.Controls.Add(btnCheck);
            pnlBottom.Controls.Add(btnFinish);
            pnlBottom.Controls.Add(btnNext);
            pnlBottom.Controls.Add(btnPrev);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 616);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1100, 64);
            pnlBottom.TabIndex = 4;
            //
            // btnReset
            //
            btnReset.Location = new Point(490, 14);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(130, 36);
            btnReset.TabIndex = 3;
            btnReset.Text = "Скинути";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += BtnReset_Click;
            //
            // btnCheck
            //
            btnCheck.BackColor = Color.FromArgb(46, 125, 50);
            btnCheck.Cursor = Cursors.Hand;
            btnCheck.FlatAppearance.BorderSize = 0;
            btnCheck.FlatStyle = FlatStyle.Flat;
            btnCheck.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnCheck.ForeColor = Color.White;
            btnCheck.Location = new Point(330, 14);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(150, 36);
            btnCheck.TabIndex = 2;
            btnCheck.Text = "Перевірити";
            btnCheck.UseVisualStyleBackColor = false;
            btnCheck.Click += BtnCheck_Click;
            //
            // btnFinish
            //
            btnFinish.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFinish.BackColor = Color.FromArgb(46, 117, 182);
            btnFinish.Cursor = Cursors.Hand;
            btnFinish.FlatAppearance.BorderSize = 0;
            btnFinish.FlatStyle = FlatStyle.Flat;
            btnFinish.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnFinish.ForeColor = Color.White;
            btnFinish.Location = new Point(910, 14);
            btnFinish.Name = "btnFinish";
            btnFinish.Size = new Size(170, 36);
            btnFinish.TabIndex = 4;
            btnFinish.Text = "Завершити тест";
            btnFinish.UseVisualStyleBackColor = false;
            btnFinish.Click += BtnFinish_Click;
            //
            // btnNext
            //
            btnNext.Location = new Point(158, 14);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(130, 36);
            btnNext.TabIndex = 1;
            btnNext.Text = "Далі →";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += BtnNext_Click;
            //
            // btnPrev
            //
            btnPrev.Location = new Point(20, 14);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(130, 36);
            btnPrev.TabIndex = 0;
            btnPrev.Text = "← Назад";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += BtnPrev_Click;
            //
            // TestForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1100, 680);
            Controls.Add(pnlContent);
            Controls.Add(pnlTheory);
            Controls.Add(pnlQuestionHeader);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1000, 640);
            Name = "TestForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Тестування з програмування на C#";
            FormClosing += TestForm_FormClosing;
            FormClosed += TestForm_FormClosed;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlQuestionHeader.ResumeLayout(false);
            pnlTheory.ResumeLayout(false);
            pnlTheory.PerformLayout();
            tlpTheory.ResumeLayout(false);
            tlpTheory.PerformLayout();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblStudent;
        private FlowLayoutPanel flpSteps;
        private Label lblTimer;
        private Panel pnlQuestionHeader;
        private Label lblQuestionInfo;
        private Label lblTask;
        private Panel pnlContent;
        private Panel pnlTheory;
        private TableLayoutPanel tlpTheory;
        private Label lblTheoryTitle;
        private Label lblTheory;
        private Label lblCheckTitle;
        private Label lblCheckSummary;
        private Label lblExplanation;
        private Panel pnlBottom;
        private Button btnPrev;
        private Button btnNext;
        private Button btnCheck;
        private Button btnReset;
        private Button btnFinish;
    }
}

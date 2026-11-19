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
            tabInfo = new TabControl();
            tabTheory = new TabPage();
            rtbTheory = new RichTextBox();
            tabCheck = new TabPage();
            rtbCheck = new RichTextBox();
            btnShowAnswer = new Button();
            pnlCheckGap = new Panel();
            pnlCheck = new Panel();
            lblCheckHint = new Label();
            lblCheckSummary = new Label();
            pnlBottom = new Panel();
            btnReset = new Button();
            btnCheck = new Button();
            btnFinish = new Button();
            btnNext = new Button();
            btnPrev = new Button();
            pnlTop.SuspendLayout();
            pnlQuestionHeader.SuspendLayout();
            pnlTheory.SuspendLayout();
            tabInfo.SuspendLayout();
            tabTheory.SuspendLayout();
            tabCheck.SuspendLayout();
            pnlCheck.SuspendLayout();
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
            pnlTop.Size = new Size(1180, 56);
            pnlTop.TabIndex = 0;
            //
            // lblTimer
            //
            lblTimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTimer.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTimer.ForeColor = Color.White;
            lblTimer.Location = new Point(768, 12);
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
            flpSteps.Location = new Point(944, 9);
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
            pnlQuestionHeader.Size = new Size(1180, 88);
            pnlQuestionHeader.TabIndex = 1;
            //
            // lblTask
            //
            lblTask.Dock = DockStyle.Fill;
            lblTask.Font = new Font("Segoe UI", 12F);
            lblTask.ForeColor = Color.FromArgb(25, 35, 48);
            lblTask.Location = new Point(20, 34);
            lblTask.Name = "lblTask";
            lblTask.Size = new Size(1140, 48);
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
            lblQuestionInfo.Size = new Size(1140, 24);
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
            pnlContent.Size = new Size(820, 472);
            pnlContent.TabIndex = 2;
            //
            // pnlTheory
            //
            pnlTheory.BackColor = Color.FromArgb(255, 249, 232);
            pnlTheory.Controls.Add(tabInfo);
            pnlTheory.Controls.Add(pnlCheckGap);
            pnlTheory.Controls.Add(pnlCheck);
            pnlTheory.Dock = DockStyle.Right;
            pnlTheory.Location = new Point(820, 144);
            pnlTheory.Name = "pnlTheory";
            pnlTheory.Padding = new Padding(8);
            pnlTheory.Size = new Size(360, 472);
            pnlTheory.TabIndex = 3;
            //
            // tabInfo
            //
            tabInfo.Controls.Add(tabTheory);
            tabInfo.Controls.Add(tabCheck);
            tabInfo.Dock = DockStyle.Fill;
            tabInfo.Location = new Point(8, 116);
            tabInfo.Name = "tabInfo";
            tabInfo.SelectedIndex = 0;
            tabInfo.Size = new Size(344, 364);
            tabInfo.TabIndex = 2;
            //
            // tabTheory
            //
            tabTheory.BackColor = Color.FromArgb(255, 249, 232);
            tabTheory.Controls.Add(rtbTheory);
            tabTheory.Location = new Point(4, 26);
            tabTheory.Name = "tabTheory";
            tabTheory.Padding = new Padding(6);
            tabTheory.Size = new Size(336, 334);
            tabTheory.TabIndex = 0;
            tabTheory.Text = "Теорія";
            //
            // rtbTheory
            //
            rtbTheory.BackColor = Color.FromArgb(255, 249, 232);
            rtbTheory.BorderStyle = BorderStyle.None;
            rtbTheory.DetectUrls = false;
            rtbTheory.Dock = DockStyle.Fill;
            rtbTheory.ForeColor = Color.FromArgb(25, 35, 48);
            rtbTheory.Location = new Point(6, 6);
            rtbTheory.Name = "rtbTheory";
            rtbTheory.ReadOnly = true;
            rtbTheory.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbTheory.Size = new Size(324, 322);
            rtbTheory.TabIndex = 0;
            rtbTheory.TabStop = false;
            rtbTheory.Text = "";
            //
            // tabCheck
            //
            tabCheck.BackColor = Color.FromArgb(255, 249, 232);
            tabCheck.Controls.Add(rtbCheck);
            tabCheck.Controls.Add(btnShowAnswer);
            tabCheck.Location = new Point(4, 26);
            tabCheck.Name = "tabCheck";
            tabCheck.Padding = new Padding(6);
            tabCheck.Size = new Size(336, 334);
            tabCheck.TabIndex = 1;
            tabCheck.Text = "Перевірка";
            //
            // rtbCheck
            //
            rtbCheck.BackColor = Color.FromArgb(255, 249, 232);
            rtbCheck.BorderStyle = BorderStyle.None;
            rtbCheck.DetectUrls = false;
            rtbCheck.Dock = DockStyle.Fill;
            rtbCheck.ForeColor = Color.FromArgb(25, 35, 48);
            rtbCheck.Location = new Point(6, 6);
            rtbCheck.Name = "rtbCheck";
            rtbCheck.ReadOnly = true;
            rtbCheck.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbCheck.Size = new Size(324, 284);
            rtbCheck.TabIndex = 0;
            rtbCheck.TabStop = false;
            rtbCheck.Text = "";
            //
            // btnShowAnswer
            //
            btnShowAnswer.Dock = DockStyle.Bottom;
            btnShowAnswer.Enabled = false;
            btnShowAnswer.Location = new Point(6, 290);
            btnShowAnswer.Name = "btnShowAnswer";
            btnShowAnswer.Size = new Size(324, 38);
            btnShowAnswer.TabIndex = 1;
            btnShowAnswer.Text = "Показати правильну відповідь";
            btnShowAnswer.UseVisualStyleBackColor = true;
            btnShowAnswer.Click += BtnShowAnswer_Click;
            //
            // pnlCheckGap
            //
            pnlCheckGap.Dock = DockStyle.Top;
            pnlCheckGap.Location = new Point(8, 108);
            pnlCheckGap.Name = "pnlCheckGap";
            pnlCheckGap.Size = new Size(344, 8);
            pnlCheckGap.TabIndex = 1;
            pnlCheckGap.Visible = false;
            //
            // pnlCheck
            //
            pnlCheck.BackColor = Color.FromArgb(226, 243, 228);
            pnlCheck.Controls.Add(lblCheckHint);
            pnlCheck.Controls.Add(lblCheckSummary);
            pnlCheck.Dock = DockStyle.Top;
            pnlCheck.Location = new Point(8, 8);
            pnlCheck.Name = "pnlCheck";
            pnlCheck.Padding = new Padding(10, 8, 10, 6);
            pnlCheck.Size = new Size(344, 100);
            pnlCheck.TabIndex = 0;
            pnlCheck.Visible = false;
            //
            // lblCheckHint
            //
            lblCheckHint.Dock = DockStyle.Fill;
            lblCheckHint.Font = new Font("Segoe UI", 9.5F);
            lblCheckHint.ForeColor = Color.FromArgb(60, 72, 86);
            lblCheckHint.Location = new Point(10, 34);
            lblCheckHint.Name = "lblCheckHint";
            lblCheckHint.Size = new Size(324, 60);
            lblCheckHint.TabIndex = 1;
            lblCheckHint.Text = "Підказка після перевірки";
            //
            // lblCheckSummary
            //
            lblCheckSummary.Dock = DockStyle.Top;
            lblCheckSummary.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblCheckSummary.Location = new Point(10, 8);
            lblCheckSummary.Name = "lblCheckSummary";
            lblCheckSummary.Size = new Size(324, 26);
            lblCheckSummary.TabIndex = 0;
            lblCheckSummary.Text = "Результат перевірки";
            lblCheckSummary.UseMnemonic = false;
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
            pnlBottom.Size = new Size(1180, 64);
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
            btnFinish.Location = new Point(990, 14);
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
            ClientSize = new Size(1180, 680);
            Controls.Add(pnlContent);
            Controls.Add(pnlTheory);
            Controls.Add(pnlQuestionHeader);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1060, 640);
            Name = "TestForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Тестування з програмування на C#";
            FormClosing += TestForm_FormClosing;
            FormClosed += TestForm_FormClosed;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlQuestionHeader.ResumeLayout(false);
            pnlTheory.ResumeLayout(false);
            tabInfo.ResumeLayout(false);
            tabTheory.ResumeLayout(false);
            tabCheck.ResumeLayout(false);
            pnlCheck.ResumeLayout(false);
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
        private Panel pnlCheck;
        private Label lblCheckSummary;
        private Label lblCheckHint;
        private Panel pnlCheckGap;
        private TabControl tabInfo;
        private TabPage tabTheory;
        private RichTextBox rtbTheory;
        private TabPage tabCheck;
        private RichTextBox rtbCheck;
        private Button btnShowAnswer;
        private Panel pnlBottom;
        private Button btnPrev;
        private Button btnNext;
        private Button btnCheck;
        private Button btnReset;
        private Button btnFinish;
    }
}

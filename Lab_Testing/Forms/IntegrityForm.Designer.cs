namespace Lab_Testing
{
    partial class IntegrityForm
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
            lblSubtitle = new Label();
            lblTitle = new Label();
            rtbRules = new RichTextBox();
            pnlConfirm = new Panel();
            chkHonest = new CheckBox();
            btnStart = new Button();
            btnCancel = new Button();
            pnlTop.SuspendLayout();
            pnlConfirm.SuspendLayout();
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
            pnlTop.Size = new Size(640, 72);
            pnlTop.TabIndex = 0;
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSubtitle.ForeColor = Color.FromArgb(214, 228, 242);
            lblSubtitle.Location = new Point(26, 44);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(372, 17);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Прочитайте уважно — так ви знатимете, що записується під час тесту";
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(22, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(229, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Правила контрольної";
            //
            // rtbRules
            //
            rtbRules.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbRules.BackColor = Color.FromArgb(245, 247, 250);
            rtbRules.BorderStyle = BorderStyle.None;
            rtbRules.Font = new Font("Segoe UI", 10.5F);
            rtbRules.Location = new Point(24, 88);
            rtbRules.Name = "rtbRules";
            rtbRules.ReadOnly = true;
            rtbRules.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbRules.Size = new Size(592, 404);
            rtbRules.TabIndex = 1;
            rtbRules.TabStop = false;
            rtbRules.Text = "";
            //
            // pnlConfirm
            //
            pnlConfirm.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlConfirm.BackColor = Color.FromArgb(255, 249, 232);
            pnlConfirm.Controls.Add(chkHonest);
            pnlConfirm.Location = new Point(24, 502);
            pnlConfirm.Name = "pnlConfirm";
            pnlConfirm.Size = new Size(592, 48);
            pnlConfirm.TabIndex = 2;
            //
            // chkHonest
            //
            chkHonest.AutoSize = true;
            chkHonest.Cursor = Cursors.Hand;
            chkHonest.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            chkHonest.ForeColor = Color.FromArgb(31, 78, 121);
            chkHonest.Location = new Point(14, 12);
            chkHonest.Name = "chkHonest";
            chkHonest.Size = new Size(309, 24);
            chkHonest.TabIndex = 0;
            chkHonest.Text = "Я проходжу тест самостійно і чесно";
            chkHonest.UseVisualStyleBackColor = true;
            chkHonest.CheckedChanged += ChkHonest_CheckedChanged;
            //
            // btnStart
            //
            btnStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnStart.BackColor = Color.FromArgb(46, 125, 50);
            btnStart.Cursor = Cursors.Hand;
            btnStart.DialogResult = DialogResult.OK;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(316, 564);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(190, 38);
            btnStart.TabIndex = 3;
            btnStart.Text = "Почати контрольну";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += BtnStart_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(516, 564);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 38);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Скасувати";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // IntegrityForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            CancelButton = btnCancel;
            ClientSize = new Size(640, 618);
            Controls.Add(btnCancel);
            Controls.Add(btnStart);
            Controls.Add(pnlConfirm);
            Controls.Add(rtbRules);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "IntegrityForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Правила контрольної";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlConfirm.ResumeLayout(false);
            pnlConfirm.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblTitle;
        private Label lblSubtitle;
        private RichTextBox rtbRules;
        private Panel pnlConfirm;
        private CheckBox chkHonest;
        private Button btnStart;
        private Button btnCancel;
    }
}

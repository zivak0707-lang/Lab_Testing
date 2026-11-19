namespace Lab_Testing
{
    partial class AboutForm
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
            lblLogo = new Label();
            lblTitle = new Label();
            lblVersion = new Label();
            lblDescription = new Label();
            lblAuthor = new Label();
            lblTech = new Label();
            btnOk = new Button();
            SuspendLayout();
            //
            // lblLogo
            //
            lblLogo.BackColor = Color.FromArgb(46, 117, 182);
            lblLogo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.Location = new Point(24, 24);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(72, 72);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "C#";
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblTitle
            //
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.Location = new Point(112, 22);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(344, 54);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Тестування з програмування на C#";
            //
            // lblVersion
            //
            lblVersion.AutoSize = true;
            lblVersion.ForeColor = Color.FromArgb(96, 108, 122);
            lblVersion.Location = new Point(114, 77);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(87, 19);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "Версія 0.1.0";
            //
            // lblDescription
            //
            lblDescription.Location = new Point(24, 116);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(432, 64);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "Додаткове завдання з дисципліни «Інструментальні засоби візуального програмування»: проєкт «Тестування» з використанням методів Drag-and-Drop.";
            //
            // lblAuthor
            //
            lblAuthor.AutoSize = true;
            lblAuthor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAuthor.Location = new Point(24, 190);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(217, 19);
            lblAuthor.TabIndex = 4;
            lblAuthor.Text = "Автор: Сергій Зівак, група 471";
            //
            // lblTech
            //
            lblTech.AutoSize = true;
            lblTech.ForeColor = Color.FromArgb(96, 108, 122);
            lblTech.Location = new Point(24, 216);
            lblTech.Name = "lblTech";
            lblTech.Size = new Size(240, 19);
            lblTech.TabIndex = 5;
            lblTech.Text = "C# · Windows Forms · .NET 10 · 2026";
            //
            // btnOk
            //
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(356, 252);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(100, 32);
            btnOk.TabIndex = 6;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            //
            // AboutForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnOk;
            ClientSize = new Size(480, 300);
            Controls.Add(btnOk);
            Controls.Add(lblTech);
            Controls.Add(lblAuthor);
            Controls.Add(lblDescription);
            Controls.Add(lblVersion);
            Controls.Add(lblTitle);
            Controls.Add(lblLogo);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Про програму";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLogo;
        private Label lblTitle;
        private Label lblVersion;
        private Label lblDescription;
        private Label lblAuthor;
        private Label lblTech;
        private Button btnOk;
    }
}

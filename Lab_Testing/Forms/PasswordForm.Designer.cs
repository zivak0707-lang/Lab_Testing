namespace Lab_Testing
{
    partial class PasswordForm
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
            lblLock = new Label();
            lblPrompt = new Label();
            lblHint = new Label();
            txtPassword = new TextBox();
            lblError = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            //
            // lblLock
            //
            lblLock.Font = new Font("Segoe MDL2 Assets", 26F);
            lblLock.ForeColor = Color.FromArgb(31, 78, 121);
            lblLock.Location = new Point(20, 20);
            lblLock.Name = "lblLock";
            lblLock.Size = new Size(52, 52);
            lblLock.TabIndex = 0;
            lblLock.Text = "";
            lblLock.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblPrompt
            //
            lblPrompt.AutoSize = true;
            lblPrompt.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblPrompt.ForeColor = Color.FromArgb(31, 78, 121);
            lblPrompt.Location = new Point(82, 20);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(152, 21);
            lblPrompt.TabIndex = 1;
            lblPrompt.Text = "Режим викладача";
            //
            // lblHint
            //
            lblHint.ForeColor = Color.FromArgb(60, 72, 86);
            lblHint.Location = new Point(82, 46);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(316, 40);
            lblHint.TabIndex = 2;
            lblHint.Text = "Введіть пароль, щоб переглянути результати учнів.";
            //
            // txtPassword
            //
            txtPassword.Location = new Point(24, 98);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(374, 25);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            //
            // lblError
            //
            lblError.ForeColor = Color.FromArgb(198, 40, 40);
            lblError.Location = new Point(24, 128);
            lblError.Name = "lblError";
            lblError.Size = new Size(374, 20);
            lblError.TabIndex = 4;
            lblError.Text = "Неправильний пароль. Спробуйте ще раз.";
            lblError.Visible = false;
            //
            // btnOk
            //
            btnOk.Location = new Point(188, 158);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(100, 34);
            btnOk.TabIndex = 5;
            btnOk.Text = "Увійти";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(298, 158);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Скасувати";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // PasswordForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnCancel;
            ClientSize = new Size(422, 210);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(lblError);
            Controls.Add(txtPassword);
            Controls.Add(lblHint);
            Controls.Add(lblPrompt);
            Controls.Add(lblLock);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PasswordForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Вхід для викладача";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLock;
        private Label lblPrompt;
        private Label lblHint;
        private TextBox txtPassword;
        private Label lblError;
        private Button btnOk;
        private Button btnCancel;
    }
}

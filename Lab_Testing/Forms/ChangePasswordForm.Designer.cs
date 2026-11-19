namespace Lab_Testing
{
    partial class ChangePasswordForm
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
            lblTitle = new Label();
            lblCurrent = new Label();
            txtCurrent = new TextBox();
            lblNew = new Label();
            txtNew = new TextBox();
            lblRepeat = new Label();
            txtRepeat = new TextBox();
            lblNote = new Label();
            lblError = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 78, 121);
            lblTitle.Location = new Point(22, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(209, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Зміна пароля викладача";
            //
            // lblCurrent
            //
            lblCurrent.AutoSize = true;
            lblCurrent.Location = new Point(24, 61);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.Size = new Size(122, 19);
            lblCurrent.TabIndex = 1;
            lblCurrent.Text = "Поточний пароль:";
            //
            // txtCurrent
            //
            txtCurrent.Location = new Point(212, 58);
            txtCurrent.Name = "txtCurrent";
            txtCurrent.Size = new Size(206, 25);
            txtCurrent.TabIndex = 2;
            txtCurrent.UseSystemPasswordChar = true;
            //
            // lblNew
            //
            lblNew.AutoSize = true;
            lblNew.Location = new Point(24, 99);
            lblNew.Name = "lblNew";
            lblNew.Size = new Size(100, 19);
            lblNew.TabIndex = 3;
            lblNew.Text = "Новий пароль:";
            //
            // txtNew
            //
            txtNew.Location = new Point(212, 96);
            txtNew.Name = "txtNew";
            txtNew.Size = new Size(206, 25);
            txtNew.TabIndex = 4;
            txtNew.UseSystemPasswordChar = true;
            //
            // lblRepeat
            //
            lblRepeat.AutoSize = true;
            lblRepeat.Location = new Point(24, 137);
            lblRepeat.Name = "lblRepeat";
            lblRepeat.Size = new Size(165, 19);
            lblRepeat.TabIndex = 5;
            lblRepeat.Text = "Повторіть новий пароль:";
            //
            // txtRepeat
            //
            txtRepeat.Location = new Point(212, 134);
            txtRepeat.Name = "txtRepeat";
            txtRepeat.Size = new Size(206, 25);
            txtRepeat.TabIndex = 6;
            txtRepeat.UseSystemPasswordChar = true;
            //
            // lblNote
            //
            lblNote.Font = new Font("Segoe UI", 9F);
            lblNote.ForeColor = Color.FromArgb(96, 108, 122);
            lblNote.Location = new Point(24, 170);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(394, 34);
            lblNote.TabIndex = 7;
            lblNote.Text = "Щонайменше 6 символів. Новий пароль зберігається на цьому комп'ютері.";
            //
            // lblError
            //
            lblError.ForeColor = Color.FromArgb(198, 40, 40);
            lblError.Location = new Point(24, 206);
            lblError.Name = "lblError";
            lblError.Size = new Size(394, 20);
            lblError.TabIndex = 8;
            lblError.Text = "Помилка";
            lblError.Visible = false;
            //
            // btnSave
            //
            btnSave.Location = new Point(208, 234);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 34);
            btnSave.TabIndex = 9;
            btnSave.Text = "Зберегти";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(318, 234);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Скасувати";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // ChangePasswordForm
            //
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnCancel;
            ClientSize = new Size(442, 286);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblError);
            Controls.Add(lblNote);
            Controls.Add(txtRepeat);
            Controls.Add(lblRepeat);
            Controls.Add(txtNew);
            Controls.Add(lblNew);
            Controls.Add(txtCurrent);
            Controls.Add(lblCurrent);
            Controls.Add(lblTitle);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangePasswordForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Зміна пароля";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Label lblCurrent;
        private TextBox txtCurrent;
        private Label lblNew;
        private TextBox txtNew;
        private Label lblRepeat;
        private TextBox txtRepeat;
        private Label lblNote;
        private Label lblError;
        private Button btnSave;
        private Button btnCancel;
    }
}

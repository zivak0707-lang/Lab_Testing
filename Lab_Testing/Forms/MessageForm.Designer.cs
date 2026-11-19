namespace Lab_Testing
{
    partial class MessageForm
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
            picIcon = new PictureBox();
            lblText = new Label();
            pnlButtons = new Panel();
            flpButtons = new FlowLayoutPanel();
            btnNo = new Button();
            btnYes = new Button();
            btnOk = new Button();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            pnlButtons.SuspendLayout();
            flpButtons.SuspendLayout();
            SuspendLayout();
            //
            // picIcon
            //
            picIcon.Location = new Point(22, 22);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(36, 36);
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picIcon.TabIndex = 0;
            picIcon.TabStop = false;
            //
            // lblText
            //
            lblText.AutoSize = true;
            lblText.Location = new Point(72, 22);
            lblText.MaximumSize = new Size(380, 0);
            lblText.Name = "lblText";
            lblText.Size = new Size(110, 19);
            lblText.TabIndex = 1;
            lblText.Text = "Текст повідомлення";
            lblText.UseMnemonic = false;
            //
            // pnlButtons
            //
            pnlButtons.BackColor = Color.FromArgb(240, 242, 245);
            pnlButtons.Controls.Add(flpButtons);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 96);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(10, 10, 14, 10);
            pnlButtons.Size = new Size(474, 54);
            pnlButtons.TabIndex = 2;
            //
            // flpButtons
            //
            flpButtons.Controls.Add(btnNo);
            flpButtons.Controls.Add(btnYes);
            flpButtons.Controls.Add(btnOk);
            flpButtons.Dock = DockStyle.Fill;
            flpButtons.FlowDirection = FlowDirection.RightToLeft;
            flpButtons.Location = new Point(10, 10);
            flpButtons.Name = "flpButtons";
            flpButtons.Size = new Size(450, 34);
            flpButtons.TabIndex = 0;
            flpButtons.WrapContents = false;
            //
            // btnNo
            //
            btnNo.DialogResult = DialogResult.No;
            btnNo.Location = new Point(353, 0);
            btnNo.Margin = new Padding(8, 0, 0, 0);
            btnNo.Name = "btnNo";
            btnNo.Size = new Size(97, 32);
            btnNo.TabIndex = 2;
            btnNo.Text = "Ні";
            btnNo.UseVisualStyleBackColor = true;
            //
            // btnYes
            //
            btnYes.DialogResult = DialogResult.Yes;
            btnYes.Location = new Point(248, 0);
            btnYes.Margin = new Padding(8, 0, 0, 0);
            btnYes.Name = "btnYes";
            btnYes.Size = new Size(97, 32);
            btnYes.TabIndex = 1;
            btnYes.Text = "Так";
            btnYes.UseVisualStyleBackColor = true;
            //
            // btnOk
            //
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(143, 0);
            btnOk.Margin = new Padding(8, 0, 0, 0);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(97, 32);
            btnOk.TabIndex = 0;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            //
            // MessageForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(474, 150);
            Controls.Add(pnlButtons);
            Controls.Add(lblText);
            Controls.Add(picIcon);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MessageForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Повідомлення";
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            pnlButtons.ResumeLayout(false);
            flpButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picIcon;
        private Label lblText;
        private Panel pnlButtons;
        private FlowLayoutPanel flpButtons;
        private Button btnOk;
        private Button btnYes;
        private Button btnNo;
    }
}

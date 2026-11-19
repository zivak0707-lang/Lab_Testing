namespace Lab_Testing;

// Вхід у режим викладача за паролем
public partial class PasswordForm : Form
{
    public PasswordForm()
    {
        InitializeComponent();
    }

    // «Увійти»: правильний пароль — закриваємо вікно з OK, неправильний — показуємо помилку
    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (TeacherPassword.Verify(txtPassword.Text))
        {
            DialogResult = DialogResult.OK;
            return;
        }

        lblError.Visible = true;
        txtPassword.SelectAll();
        txtPassword.Focus();
    }
}

namespace Lab_Testing;

// Зміна пароля режиму викладача
public partial class ChangePasswordForm : Form
{
    private const int MinLength = 6;

    public ChangePasswordForm()
    {
        InitializeComponent();
    }

    // «Зберегти»: перевіряємо поточний пароль і новий, потім записуємо новий
    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!TeacherPassword.Verify(txtCurrent.Text))
        {
            ShowError("Поточний пароль неправильний.", txtCurrent);
            return;
        }

        if (txtNew.Text.Length < MinLength)
        {
            ShowError($"Новий пароль має містити щонайменше {MinLength} символів.", txtNew);
            return;
        }

        if (txtNew.Text != txtRepeat.Text)
        {
            ShowError("Новий пароль і повтор не збігаються.", txtRepeat);
            return;
        }

        try
        {
            TeacherPassword.Change(txtNew.Text);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError("Не вдалося зберегти пароль: " + ex.Message, txtNew);
            return;
        }

        Dialogs.Info(this, "Пароль змінено. Наступного разу входьте з новим паролем.", "Зміна пароля");
        DialogResult = DialogResult.OK;
    }

    private void ShowError(string message, TextBox field)
    {
        lblError.Text = message;
        lblError.Visible = true;
        field.SelectAll();
        field.Focus();
    }
}

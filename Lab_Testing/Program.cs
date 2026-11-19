namespace Lab_Testing;

internal static class Program
{
    // Точка входу програми
    [STAThread]
    private static void Main()
    {
        // Стандартні налаштування WinForms (візуальні стилі, DPI) з файлу проєкту
        ApplicationConfiguration.Initialize();

        QuestionBankData bank;
        string bankSource;

        try
        {
            bank = QuestionBank.Load(out bankSource);
        }
        catch (BankException ex)
        {
            // Банк з помилками: показуємо, що саме не так, і не запускаємо тест
            MessageBox.Show("Не вдалося завантажити банк питань.\n\n" + ex.Message,
                AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new StartForm(bank, bankSource));
    }
}

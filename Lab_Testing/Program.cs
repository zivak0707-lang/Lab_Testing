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
            Dialogs.Error(null, "Не вдалося завантажити банк питань.\n\n" + ex.Message, AppInfo.Title);
            return;
        }

        Application.Run(new StartForm(bank, bankSource));
    }
}

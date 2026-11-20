namespace Lab_Testing;

internal static class Program
{
    // Точка входу програми
    [STAThread]
    private static void Main()
    {
        // Стандартні налаштування WinForms (візуальні стилі, DPI) з файлу проєкту
        ApplicationConfiguration.Initialize();

        // Непередбачені помилки — у журнал і зрозуміле повідомлення (під час контрольної — зі збереженням відповідей)
        CrashHandler.Install();

        // Під час контрольної ще одну копію програми не відкриваємо: у ній можна було б
        // перевіряти відповіді в «Навчанні». Копія з контрольною запише цю спробу
        if (InstanceGuard.ControlIsRunning)
        {
            InstanceGuard.NotifyControl();
            Dialogs.Warning(null, "Зараз на цьому комп'ютері триває контрольна.\n\n" +
                                  "Ще одну копію програми можна відкрити лише після її завершення. " +
                                  "Спробу відкрити програму записано — вчитель її побачить.",
                AppInfo.Title);
            return;
        }

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

        AppLog.Info("запуск", $"версія {AppInfo.Version}; банк: {bankSource}");
        InstanceGuard.Register();

        try
        {
            Application.Run(new StartForm(bank, bankSource));
        }
        finally
        {
            InstanceGuard.Unregister();
        }
    }
}

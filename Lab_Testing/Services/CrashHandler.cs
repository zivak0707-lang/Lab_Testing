namespace Lab_Testing;

// Непередбачені помилки програми. Замість стандартного англійського вікна WinForms (або «завислого» екрана
// під час контрольної на весь екран): запис у журнал і зрозуміле повідомлення українською.
// Якщо помилка сталася під час контрольної — контрольна завершується, а відповіді зберігаються у файл
public static class CrashHandler
{
    private static bool handling;   // помилка під час обробки помилки — лише журнал (без нескінченного циклу)

    // Викликається в Program.Main до створення першого вікна
    public static void Install()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Handle(e.Exception);

        // Помилка у фоновому потоці: програма після цього закривається (так влаштовано .NET), тож лише записуємо
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                AppLog.Error("фоновий потік", ex);
            }
        };
    }

    // Помилка в потоці вікон: програма продовжує працювати
    public static void Handle(Exception ex)
    {
        AppLog.Error("непередбачена помилка", ex);

        if (handling)
        {
            return;
        }

        handling = true;

        try
        {
            if (TestForm.RunningControl is TestForm test)
            {
                test.EmergencyFinish();
            }
            else
            {
                Dialogs.Error(Form.ActiveForm,
                    "Сталася непередбачена помилка програми. Можна продовжувати роботу; якщо помилка повторюється — " +
                    "перезапустіть програму.\n\nПодробиці записано в журнал:\n" + AppLog.FilePath,
                    "Помилка програми");
            }
        }
        catch (Exception inner)
        {
            AppLog.Error("під час обробки помилки", inner);
        }
        finally
        {
            handling = false;
        }
    }
}

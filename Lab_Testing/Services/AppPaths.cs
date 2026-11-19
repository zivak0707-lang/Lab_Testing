namespace Lab_Testing;

// Тека, де програма зберігає свої дані на цьому комп'ютері:
// змінений пароль викладача, ключ підпису викладача, резервні копії результатів.
// За замовчуванням — %AppData%\Lab_Testing. Змінна середовища LAB_TESTING_DATA дозволяє вказати іншу теку
// (так робить утиліта знімків, щоб не чіпати дані на комп'ютері, де її запускають)
public static class AppPaths
{
    public static string DataFolder
    {
        get
        {
            string? custom = Environment.GetEnvironmentVariable("LAB_TESTING_DATA");

            return string.IsNullOrWhiteSpace(custom)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lab_Testing")
                : custom;
        }
    }
}

namespace Lab_Testing;

// Відомості про програму (стартова сторінка і вікно «Про програму»)
public static class AppInfo
{
    public const string Title = "Тестування з програмування на C#";
    public const string Author = "Сергій Зівак";
    public const string Group = "471";
    public const string Discipline = "Інструментальні засоби візуального програмування";

    // Версія береться з <Version> у файлі Lab_Testing.csproj
    public static string Version
    {
        get
        {
            Version v = typeof(AppInfo).Assembly.GetName().Version ?? new Version(1, 0, 0);
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }
}

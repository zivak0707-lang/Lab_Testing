namespace Lab_Testing;

// Відомості про програму (стартова сторінка і вікно «Про програму»)
public static class AppInfo
{
    public const string Title = "Тестування з програмування на C#";
    public const string Author = "Сергій Зівак";
    public const string Group = "471";
    public const string Discipline = "Інструментальні засоби візуального програмування";

    // Іконка програми для заголовків вікон і панелі задач (вбудована в exe як ресурс)
    public static Icon? Icon { get; } = LoadIcon();

    private static Icon? LoadIcon()
    {
        using Stream? stream = typeof(AppInfo).Assembly.GetManifestResourceStream("Lab_Testing.app.ico");
        return stream != null ? new Icon(stream) : null;
    }

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

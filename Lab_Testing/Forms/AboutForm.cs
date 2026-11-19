namespace Lab_Testing;

// Вікно «Про програму»: назва, версія, автор
public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();

        lblTitle.Text = AppInfo.Title;
        lblVersion.Text = "Версія " + AppInfo.Version;
        lblDescription.Text = $"Додаткове завдання з дисципліни «{AppInfo.Discipline}»: " +
                              "проєкт «Тестування» з використанням методів Drag-and-Drop.";
        lblAuthor.Text = $"Автор: {AppInfo.Author}, група {AppInfo.Group}";
    }
}

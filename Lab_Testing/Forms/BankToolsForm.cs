using System.IO.Compression;
using System.Text;

namespace Lab_Testing;

// Інструменти вчителя: банк питань (редагування, перевірка, підготовка для учнів) і ключ підпису
public partial class BankToolsForm : Form
{
    public const string StudentZipName = "Lab_Testing_для_учнів.zip";

    public BankToolsForm(QuestionBankData bank, string bankSource)
    {
        InitializeComponent();

        lblBankInfo.Text = $"Чинний банк: {bank.Questions.Count} питань ({bankSource}). " +
                           "Для учнів банк шифрується разом з вашим ключем підпису і паролем режиму викладача.";
        UpdateKeyInfo();
        RichTextFormatter.ShowHint(rtbLog, "Тут з'являтиметься результат ваших дій.");
    }

    // ===================== Ключ підпису =====================

    private void UpdateKeyInfo()
    {
        TeacherKey.KeyInfo? key = TeacherKey.Current;

        lblKeyInfo.Text = key == null
            ? "Ключ ще не створено. Він з'явиться, коли ви вперше підготуєте банк для учнів, " +
              "або відновіть його з файлу копії, який вам передали."
            : $"Ключ № {key.Id}, створено {key.Created:dd.MM.yyyy HH:mm}. Ним підписуються результати учнів, " +
              "які працюють з вашим банком. Зберігайте копію ключа — без нього результати не перевірити.";
        btnExportKey.Enabled = key != null;
    }

    private void BtnExportKey_Click(object? sender, EventArgs e)
    {
        TeacherKey.KeyInfo? key = TeacherKey.Current;

        if (key == null)
        {
            return;
        }

        saveKeyDialog.FileName = $"ключ_вчителя_{key.Id}{TeacherKey.KeyFileExtension}";

        if (saveKeyDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        // Ключ не повинен лежати там, де є файли учнів: з ним можна підписати підроблений результат
        string? reason = StudentFolderReason(Path.GetDirectoryName(saveKeyDialog.FileName)!);

        if (reason != null && !Dialogs.Ask(this,
                $"Ви зберігаєте копію ключа в теку, {reason}.\n\n" +
                "Ключ не повинен потрапити до учнів: хто його має, може підписати підроблений результат. " +
                "Краще зберегти копію на флешку або в особисту теку, куди учні не мають доступу.\n\nЗберегти тут усе одно?",
                "Небезпечне місце для ключа", warning: true))
        {
            return;
        }

        TeacherKey.Export(saveKeyDialog.FileName);
        ShowLog($"## Копію ключа збережено\n- Файл: `{saveKeyDialog.FileName}`\n" +
                "- Зберігайте його в надійному місці і не передавайте учням: хто має ключ, може підписати підроблений результат.\n" +
                "- На іншому комп'ютері вчителя: «Відновити ключ з файлу…».");
    }

    private void BtnImportKey_Click(object? sender, EventArgs e)
    {
        if (openKeyDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            TeacherKey.KeyInfo key = TeacherKey.Import(openKeyDialog.FileName);
            UpdateKeyInfo();
            ShowLog($"## Ключ відновлено\n- Номер ключа: `{key.Id}`.\n" +
                    "- Тепер файли учнів, підписані цим ключем, у таблиці результатів позначаються «✔ Справжній».");
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or FormatException)
        {
            ShowLog("## Ключ не відновлено\n" + ex.Message);
        }
    }

    // ===================== Банк питань =====================

    // Чинний банк — у JSON, щоб відредагувати
    private void BtnSaveJson_Click(object? sender, EventArgs e)
    {
        SaveText("questions.json", QuestionBank.CurrentJson, "Банк збережено для редагування");
    }

    // Шаблон: по одному простому питанню кожного типу з коментарями
    private void BtnSaveTemplate_Click(object? sender, EventArgs e)
    {
        SaveText("bank_template.json", QuestionBank.ReadTemplateJson(), "Шаблон банку збережено");
    }

    private void SaveText(string fileName, string text, string title)
    {
        saveJsonDialog.FileName = fileName;

        if (saveJsonDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        File.WriteAllText(saveJsonDialog.FileName, text, new UTF8Encoding(false));
        ShowLog($"## {title}\n- Файл: `{saveJsonDialog.FileName}`\n" +
                "- Відкрийте його в Блокноті, змініть питання, потім натисніть «Перевірити файл банку…».\n" +
                "- Формат банку описано в посібнику вчителя (docs/TEACHER_GUIDE.md у репозиторії програми).");
    }

    private void BtnCheckFile_Click(object? sender, EventArgs e)
    {
        if (openJsonDialog.ShowDialog(this) == DialogResult.OK)
        {
            CheckBankFile(openJsonDialog.FileName);
        }
    }

    // Перевіряє файл банку і показує, що в ньому не так (або що все гаразд)
    public void CheckBankFile(string path)
    {
        try
        {
            QuestionBankData bank = QuestionBank.Parse(File.ReadAllText(path), Path.GetFileName(path));
            string counts = string.Join(", ", Enum.GetValues<QuestionType>()
                .Select(t => $"{t.ToUkrainian()} — {bank.Questions.Count(q => q.Type == t)}"));

            ShowLog($"## ✔ Банк правильний\n- Файл: `{path}`\n- Питань: {bank.Questions.Count} ({counts}).\n" +
                    $"- Час контрольної: {bank.Settings.ControlTimeMinutes} хв.\n" +
                    "- Тепер його можна підготувати для учнів: оберіть «банк з файлу JSON» і натисніть зелену кнопку.");
        }
        catch (BankException ex)
        {
            ShowErrors($"Файл «{Path.GetFileName(path)}» містить помилки", ex.Message);
        }
    }

    // «Підготувати банк для учнів»: questions.dat (+ ZIP з програмою) у вибрану теку
    private void BtnPrepare_Click(object? sender, EventArgs e)
    {
        string bankJson;

        if (rbCurrentBank.Checked)
        {
            bankJson = QuestionBank.CurrentJson;
        }
        else
        {
            if (openJsonDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            bankJson = File.ReadAllText(openJsonDialog.FileName);
        }

        if (folderDialog.ShowDialog(this) == DialogResult.OK)
        {
            PrepareForStudents(bankJson, folderDialog.SelectedPath);
        }
    }

    // Шифрує банк разом із ключем підпису і паролем вчителя у questions.dat; якщо можна — додає ZIP для учнів.
    // exePath — програма, яку покласти в ZIP (за замовчуванням — запущений exe)
    public void PrepareForStudents(string bankJson, string folder, string? exePath = null)
    {
        QuestionBankData bank;

        try
        {
            bank = QuestionBank.Parse(bankJson, "банк");
        }
        catch (BankException ex)
        {
            ShowErrors("Банк не підготовлено — спочатку виправте помилки", ex.Message);
            return;
        }

        TeacherKey.KeyInfo key = TeacherKey.GetOrCreate();
        TeacherPassword.PasswordHash password = TeacherPassword.Current();
        BankPackage package = new(bankJson, key.Id, key.Key, password.Salt, password.Hash, DateTime.Now, AppInfo.Version);

        string datPath = Path.Combine(folder, BankFile.FileName);
        BankFile.Write(datPath, package);
        string? zipPath = TryMakeStudentZip(folder, datPath, exePath ?? Environment.ProcessPath);
        UpdateKeyInfo();

        string zipLine = zipPath != null
            ? $"- Архів для учнів: `{zipPath}` (програма + банк).\n"
            : "- Архів для учнів не створено: так буває, коли запущено версію для розробки. " +
              "Роздайте учням `Lab_Testing.exe` і `questions.dat` разом, в одній теці.\n";

        // Копія ключа в теці для учнів — цю теку не можна роздавати цілком
        string keyWarning = Directory.EnumerateFiles(folder, "*" + TeacherKey.KeyFileExtension).Any()
            ? "- **Увага:** у цій теці лежить копія ключа (.labkey). Не роздавайте теку цілком — лише архів або два файли.\n"
            : "";

        ShowLog($"## ✔ Банк для учнів готовий ({bank.Questions.Count} питань)\n" +
                $"- Файл банку: `{datPath}`\n" + zipLine + keyWarning +
                $"- Ключ підпису № `{key.Id}`. Файли учнів з цим банком будуть позначені «✔ Справжній».\n" +
                "- Роздайте учням **архів**, а не посилання на сторінку програми: без вашого банку результати будуть «⚠ Загальний ключ».\n" +
                "- Зробіть копію ключа: «Зберегти копію ключа…».");
    }

    // Чому тека небезпечна для ключа (null — тека звичайна): туди учні здають файли або звідти роздають банк
    public static string? StudentFolderReason(string folder)
    {
        if (string.Equals(Path.GetFileName(folder.TrimEnd('\\', '/')), ResultFile.FolderName, StringComparison.OrdinalIgnoreCase))
        {
            return $"куди програма учня зберігає результати («{ResultFile.FolderName}»)";
        }

        if (!Directory.Exists(folder))
        {
            return null;
        }

        if (Directory.EnumerateFiles(folder, "Результат_*.json").Any())
        {
            return "де лежать файли результатів учнів";
        }

        if (File.Exists(Path.Combine(folder, BankFile.FileName)) || File.Exists(Path.Combine(folder, StudentZipName)))
        {
            return "де лежить банк або архів для учнів";
        }

        return null;
    }

    // ZIP «для учнів»: цей exe + questions.dat. Лише з опублікованої програми (один exe):
    // у версії для розробки поруч з exe лежить Lab_Testing.dll, і окремий exe без неї не запуститься
    private static string? TryMakeStudentZip(string folder, string datPath, string? exePath)
    {
        if (exePath == null || File.Exists(Path.Combine(Path.GetDirectoryName(exePath)!, "Lab_Testing.dll")))
        {
            return null;
        }

        string zipPath = Path.Combine(folder, StudentZipName);
        File.Delete(zipPath);

        using ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        zip.CreateEntryFromFile(exePath, "Lab_Testing.exe", CompressionLevel.Fastest);
        zip.CreateEntryFromFile(datPath, BankFile.FileName);
        return zipPath;
    }

    // ===================== Журнал дій =====================

    private void ShowLog(string markup)
    {
        RichTextFormatter.Show(rtbLog, markup);
    }

    // Помилки банку: кожна — окремим пунктом (рядок «… містить помилки:» уже є в заголовку)
    private void ShowErrors(string title, string message)
    {
        IEnumerable<string> lines = message.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.EndsWith("містить помилки:"))
            .Select(l => l.StartsWith('•') ? "- " + l[1..].Trim() : l);

        ShowLog($"## ✗ {title}\n" + string.Join("\n", lines));
    }
}

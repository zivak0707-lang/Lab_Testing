using System.Diagnostics;
using System.Globalization;

namespace Lab_Testing;

// Режим викладача: таблиця результатів учнів з перевіркою, чи файли справжні, і експорт у Excel
public partial class TeacherForm : Form
{
    private readonly QuestionBankData bank;                 // банк — звідси пояснення для подробиць
    private readonly string bankSource;                      // звідки взято банк (для вікна «Банк питань»)
    private readonly List<ResultEntry> entries = new();      // відкриті файли (у порядку відкриття)

    public TeacherForm(QuestionBankData bank, string bankSource)
    {
        InitializeComponent();
        this.bank = bank;
        this.bankSource = bankSource;

        // Ненав'язлива порада, якщо вхід виконано з типовим паролем
        pnlPasswordHint.Visible = TeacherPassword.IsDefault;

        // Числа в таблиці — по-українському (13,3); дата і час — у звичному вигляді
        CultureInfo ukrainian = CultureInfo.GetCultureInfo("uk-UA");

        foreach (DataGridViewColumn column in new[] { colQ1, colQ2, colQ3, colQ4, colQ5, colTotal })
        {
            column.DefaultCellStyle.Format = "0.#";
            column.DefaultCellStyle.FormatProvider = ukrainian;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        colDate.DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
        colDuration.DefaultCellStyle.Format = @"h\:mm\:ss";
        colGrade.DefaultCellStyle.Font = Theme.SectionFont;
        colGrade.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colAttempt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        // Підказка «перетягніть файли» лежить поверх порожньої таблиці
        dgvResults.Controls.Add(lblDropHint);

        // Діалог відкриття одразу показує теку з результатами на робочому столі (якщо вона є)
        openFileDialog.InitialDirectory = Directory.Exists(ResultFile.DesktopFolder)
            ? ResultFile.DesktopFolder
            : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        UpdateView();
    }

    // Відкриті файли (для перевірки в утиліті знімків)
    public IReadOnlyList<ResultEntry> Entries => entries;

    // ===================== Відкриття файлів =====================

    private void BtnOpenFiles_Click(object? sender, EventArgs e)
    {
        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFiles(openFileDialog.FileNames);
        }
    }

    private void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFiles(new[] { folderBrowserDialog.SelectedPath });
        }
    }

    // Додає в таблицю файли і теки (з теки беруться всі файли результатів *.json)
    public void LoadFiles(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path, "*.json"))
                {
                    AddFile(file, fromFolder: true);
                }
            }
            else if (File.Exists(path))
            {
                AddFile(path, fromFolder: false);
            }
        }

        // Таблиця сама виділяє перший рядок — знімаємо виділення, щоб було видно кольори рядків
        dgvResults.CurrentCell = null;
        dgvResults.ClearSelection();
        UpdateView();
    }

    private void AddFile(string path, bool fromFolder)
    {
        string fullPath = Path.GetFullPath(path);

        // Той самий файл вдруге не додаємо
        if (entries.Any(e => string.Equals(e.FilePath, fullPath, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        ResultEntry entry = ResultFile.Load(fullPath);

        // У теці можуть бути інші .json (не результати) — їх тихо пропускаємо;
        // а файл, який викладач вибрав сам, показуємо навіть пошкодженим
        if (fromFolder && entry.Status == ResultFileStatus.Damaged)
        {
            return;
        }

        entries.Add(entry);
        AddRow(entry);
    }

    // ===================== Drag-and-Drop файлів з Провідника =====================

    // Над таблицею тягнуть файли — дозволяємо копіювання (курсор зі знаком «+»)
    private void DgvResults_DragEnter(object? sender, DragEventArgs e)
    {
        bool files = e.Data?.GetDataPresent(DataFormats.FileDrop) == true;
        e.Effect = files ? DragDropEffects.Copy : DragDropEffects.None;
    }

    // Файли відпустили над таблицею — відкриваємо їх
    private void DgvResults_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] paths)
        {
            LoadFiles(paths);
        }
    }

    // ===================== Таблиця =====================

    // Рядок таблиці для одного файлу
    private void AddRow(ResultEntry entry)
    {
        TestResult? r = entry.Result;
        string fileName = Path.GetFileName(entry.FilePath);
        List<object> values = new() { entries.Count };

        if (r == null)
        {
            // Пошкоджений файл: даних немає — порожні клітинки
            values.Add("—");
            values.AddRange(Enumerable.Repeat<object>("", 12));   // група … порушення
        }
        else
        {
            values.AddRange(new object[] { r.StudentName, r.Group, r.FinishedAt, r.Duration, r.AttemptNumber });

            // Бали за питання 1–5 (числами — тоді таблиця правильно сортується)
            for (int i = 0; i < 5; i++)
            {
                values.Add(i < r.Questions.Count ? r.Questions[i].Score : "");
            }

            values.Add(r.TotalScore);
            values.Add(r.Grade);
            values.Add(IntegrityText.Summary(r));   // режим доброчесності: виходи з вікна тощо
        }

        // Стан файлу і примітка: що не так з файлом (довга примітка переноситься на кілька рядків)
        values.Add(ResultChecks.StatusText(entry.Status));
        values.Add(ResultChecks.Note(entry, bank.Settings));
        int index = dgvResults.Rows.Add(values.ToArray());

        DataGridViewRow row = dgvResults.Rows[index];
        row.Tag = entry;

        // Назва файлу — у підказці під курсором (у книзі Excel вона є окремим стовпцем)
        foreach (DataGridViewCell cell in row.Cells)
        {
            cell.ToolTipText = "Файл: " + fileName;
        }

        // Справжній — зелений напис; не вашим ключем — жовтий рядок; змінений — червоний; пошкоджений — сірий
        DataGridViewCell status = row.Cells[colStatus.Index];
        status.Style.Font = Theme.SectionFont;

        switch (entry.Status)
        {
            case ResultFileStatus.Valid:
                status.Style.ForeColor = Theme.Success;
                break;

            case ResultFileStatus.DefaultKey:
            case ResultFileStatus.UnknownKey:
                row.DefaultCellStyle.BackColor = Theme.PartlyBack;
                status.Style.ForeColor = Theme.Warning;
                break;

            case ResultFileStatus.Modified:
                row.DefaultCellStyle.BackColor = Theme.WrongBack;
                status.Style.ForeColor = Theme.Error;
                break;

            default:
                row.DefaultCellStyle.BackColor = Color.FromArgb(238, 240, 243);
                status.Style.ForeColor = Theme.Warning;
                break;
        }

        // Порушення доброчесності — фіолетовий напис; якщо сам файл справжній — ще й фіолетовий рядок
        // (червоний, жовтий і сірий рядки лишаються за станом файлу — він важливіший)
        if (r != null && IntegrityText.HasViolations(r))
        {
            DataGridViewCell violations = row.Cells[colViolations.Index];
            violations.Style.ForeColor = Theme.Violation;
            violations.Style.Font = Theme.SectionFont;

            if (entry.Status == ResultFileStatus.Valid)
            {
                row.DefaultCellStyle.BackColor = Theme.ViolationBack;
            }
        }
    }

    // Підказка, кнопки і рядок стану залежать від того, чи є файли
    private void UpdateView()
    {
        int valid = entries.Count(e => e.Status == ResultFileStatus.Valid);
        int otherKey = entries.Count(e => e.Status is ResultFileStatus.DefaultKey or ResultFileStatus.UnknownKey);
        int modified = entries.Count(e => e.Status == ResultFileStatus.Modified);
        int damaged = entries.Count(e => e.Status == ResultFileStatus.Damaged);
        int violations = entries.Count(e => e.Result != null && IntegrityText.HasViolations(e.Result));

        lblDropHint.Visible = entries.Count == 0;
        btnExport.Enabled = entries.Any(e => e.Result != null);
        btnExport.BackColor = btnExport.Enabled ? Theme.Success : Theme.Border;   // недоступна кнопка — сіра
        btnClear.Enabled = entries.Count > 0;
        CenterDropHint();

        lblStatus.Text = entries.Count == 0
            ? "Файлів не відкрито"
            : $"Файлів: {entries.Count}   ·   справжніх: {valid}   ·   не вашим ключем: {otherKey}   ·   " +
              $"змінених: {modified}   ·   пошкоджених: {damaged}   ·   з порушеннями: {violations}";
        lblStatus.ForeColor = otherKey + modified + damaged + violations > 0 ? Theme.Error : Theme.CardText;
    }

    private void DgvResults_Resize(object? sender, EventArgs e)
    {
        CenterDropHint();
    }

    // Сортування за стовпцем: у пошкоджених файлів клітинки порожні (рядок ""), а в інших — числа, дати чи час.
    // Стандартне порівняння не вміє порівнювати рядок з числом, тому порожні клітинки ставимо першими,
    // а однотипні значення порівнюємо як числа, дати чи час
    private void DgvResults_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
    {
        bool empty1 = e.CellValue1 is null or "";
        bool empty2 = e.CellValue2 is null or "";

        if (empty1 || empty2)
        {
            e.SortResult = empty1 == empty2 ? 0 : empty1 ? -1 : 1;
        }
        else if (e.CellValue1!.GetType() == e.CellValue2!.GetType() && e.CellValue1 is IComparable comparable)
        {
            e.SortResult = comparable.CompareTo(e.CellValue2);
        }
        else
        {
            e.SortResult = string.Compare(e.CellValue1.ToString(), e.CellValue2.ToString(), StringComparison.CurrentCulture);
        }

        // Однакові значення — за порядковим номером, щоб рядки не «стрибали»
        if (e.SortResult == 0)
        {
            e.SortResult = e.RowIndex1.CompareTo(e.RowIndex2);
        }

        e.Handled = true;
    }

    private void CenterDropHint()
    {
        lblDropHint.Location = new Point((dgvResults.ClientSize.Width - lblDropHint.Width) / 2,
                                         (dgvResults.ClientSize.Height - lblDropHint.Height) / 2);
    }

    // ===================== Подробиці результату =====================

    private void DgvResults_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && dgvResults.Rows[e.RowIndex].Tag is ResultEntry entry)
        {
            OpenDetails(entry);
        }
    }

    // Відкриває сторінку результату учня (як бачив її учень, але з позначкою, чи справжній файл)
    private void OpenDetails(ResultEntry entry)
    {
        if (entry.Result == null)
        {
            Dialogs.Warning(this, $"Файл «{Path.GetFileName(entry.FilePath)}» не вдалося прочитати: " +
                                  "він пошкоджений або це не файл результату тесту.",
                "Пошкоджений файл");
            return;
        }

        // Файл не підтверджено вашим ключем — попереджаємо, чому дані можуть бути недостовірними,
        // і показуємо, що саме в них не сходиться (якщо це вдалося визначити)
        if (entry.Status != ResultFileStatus.Valid)
        {
            List<string> mismatches = ResultChecks.FindMismatches(entry.Result, bank.Settings);
            string details = mismatches.Count > 0
                ? "\n\nЩо не сходиться:\n• " + string.Join("\n• ", mismatches)
                : entry.Status == ResultFileStatus.Modified
                    ? "\n\nЧисла у файлі між собою сходяться — змінено щось інше (ПІБ, групу, дату чи тексти)."
                    : "";

            if (!Dialogs.Ask(this, ResultChecks.WarningText(entry.Status) + details + "\n\nПоказати вміст усе одно?",
                    "Файл не підтверджено", warning: true))
            {
                return;
            }
        }

        // Пояснень у файлі немає — беремо їх із банку за номером питання
        foreach (QuestionResult question in entry.Result.Questions)
        {
            question.Explanation = bank.Questions.FirstOrDefault(q => q.Id == question.QuestionId)?.Explanation ?? "";
        }

        using ResultForm form = new(entry.Result, showReview: true, entry.FilePath);
        form.ShowForTeacher(entry.Status);
        form.ShowDialog(this);
    }

    // ===================== Експорт, очищення, пароль =====================

    // Експорт у книгу Excel (.xlsx) або CSV — у тому порядку, як зараз відсортовано таблицю
    private void BtnExport_Click(object? sender, EventArgs e)
    {
        saveFileDialog.FileName = $"Результати_{DateTime.Now:yyyy-MM-dd}.xlsx";
        saveFileDialog.FilterIndex = 1;

        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        List<ResultEntry> ordered = dgvResults.Rows.Cast<DataGridViewRow>()
            .Select(r => (ResultEntry)r.Tag!)
            .ToList();

        try
        {
            if (Path.GetExtension(saveFileDialog.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                CsvExport.Save(saveFileDialog.FileName, ordered, bank.Settings);
            }
            else
            {
                ExcelExport.Save(saveFileDialog.FileName, ordered, bank.Settings);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Dialogs.Error(this, "Не вдалося зберегти файл. Можливо, він відкритий в Excel — закрийте його і спробуйте ще раз.\n\n" +
                                ex.Message, "Експорт");
            return;
        }

        if (Dialogs.Ask(this, $"Таблицю збережено:\n{saveFileDialog.FileName}\n\nВідкрити її зараз?", "Експорт"))
        {
            Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
        }
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        entries.Clear();
        dgvResults.Rows.Clear();
        UpdateView();
    }

    private void BtnChangePassword_Click(object? sender, EventArgs e)
    {
        using ChangePasswordForm form = new();
        form.ShowDialog(this);

        // Пароль змінено — порада більше не потрібна
        pnlPasswordHint.Visible = TeacherPassword.IsDefault;
    }

    private void LnkChangePassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        BtnChangePassword_Click(sender, e);
    }

    // «✕» на пораді: сховати до наступного входу
    private void BtnCloseHint_Click(object? sender, EventArgs e)
    {
        pnlPasswordHint.Visible = false;
    }

    // Вікно «Банк питань і ключ»
    private void BtnBankTools_Click(object? sender, EventArgs e)
    {
        using BankToolsForm form = new(bank, bankSource);
        form.ShowDialog(this);
    }
}

namespace Lab_Testing;

// Сторінка результату: бали за кожне питання, оцінка за 12-бальною шкалою, розбір помилок
public partial class ResultForm : Form
{
    private readonly Font boldFont;     // жирний шрифт для балів у таблиці
    private readonly string? filePath;  // збережений файл результату (лише після контрольної)

    // showReview = false — розбір помилок вимкнено викладачем (бали й оцінку видно завжди);
    // filePath — шлях до збереженого файлу результату контрольної
    public ResultForm(TestResult result, bool showReview, string? filePath = null)
    {
        InitializeComponent();
        boldFont = new Font(rtbReview.Font, FontStyle.Bold);
        this.filePath = filePath;

        string attempt = result.AttemptNumber > 1 ? $"  ·  спроба №{result.AttemptNumber}" : "";
        lblStudent.Text = $"{result.StudentName}  ·  група {result.Group}  ·  режим «{result.Mode.ToUkrainian()}»{attempt}";
        lblDetails.Text = $"{result.FinishedAt:dd.MM.yyyy HH:mm}  ·  час проходження: {Grading.FormatDuration(result.Duration)}" +
                          result.Reason switch
                          {
                              FinishReason.TimeUp => "  ·  завершено, бо скінчився час",
                              FinishReason.ExitLimit => "  ·  завершено достроково: ліміт виходів з вікна",
                              FinishReason.ProgramError => "  ·  завершено через помилку програми",
                              _ => ""
                          };

        lblGrade.Text = result.Grade.ToString();
        lblLevel.Text = Grading.Level(result.Grade);
        lblScore.Text = $"{Grading.Format(result.TotalScore)} з {Grading.Format(result.MaxScore)} балів  ·  " +
                        $"{Grading.Format(result.Percent)} %";

        // Внизу: де лежить файл результату (контрольна) або що результат не зберігається (навчання)
        if (result.Mode == TestMode.Training)
        {
            lblFileInfo.Text = "Режим «Навчання»: результат не зберігається у файл.";
        }
        else if (filePath != null)
        {
            lblFileInfo.Text = "Файл для викладача: " + filePath;
            btnOpenFolder.Visible = true;
        }
        else
        {
            lblFileInfo.Text = "Файл результату не збережено — покажіть цю сторінку викладачеві.";
        }

        FillTable(result);
        rtbReview.Clear();

        // Режим доброчесності: що записано під час контрольної (бачать і учень, і вчитель)
        if (result.Integrity != null)
        {
            FillIntegrity(result);
        }

        if (showReview)
        {
            FillReview(result);
        }
        else
        {
            RichTextFormatter.AppendParagraph(rtbReview,
                "Розбір помилок після контрольної вимкнено викладачем. Ваші бали та оцінку показано вище.", Theme.MutedText);
        }

        rtbReview.SelectionStart = 0;   // прокрутити на початок
        rtbReview.ScrollToCaret();

        // Таблиця сама виділяє перший рядок, коли отримує фокус, — знімаємо виділення
        Shown += (_, _) =>
        {
            dgvQuestions.CurrentCell = null;
            dgvQuestions.ClearSelection();
            btnClose.Focus();
        };

        FormClosed += (_, _) => boldFont.Dispose();
    }

    // Вигляд для викладача: заголовок і внизу — чи справжній файл
    public void ShowForTeacher(ResultFileStatus status)
    {
        bool valid = status == ResultFileStatus.Valid;

        lblTitle.Text = status switch
        {
            ResultFileStatus.Valid => "Результат учня",
            ResultFileStatus.Modified => "Результат учня (файл змінено!)",
            _ => "Результат учня (справжність не підтверджено)"
        };
        lblFileInfo.Text = ResultChecks.StatusText(status) + ": " + Path.GetFileName(filePath);
        lblFileInfo.ForeColor = valid ? Theme.Success : Theme.Error;
    }

    // «Відкрити теку з файлом»: Провідник з виділеним файлом результату
    private void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        if (filePath != null && File.Exists(filePath))
        {
            ResultFile.ShowInExplorer(filePath);
        }
    }

    // Таблиця: по рядку на кожне питання; колір балів — зелений, помаранчевий або червоний
    private void FillTable(TestResult result)
    {
        for (int i = 0; i < result.Questions.Count; i++)
        {
            QuestionResult q = result.Questions[i];
            int row = dgvQuestions.Rows.Add(i + 1, q.Type.ToUkrainian(), q.Title, q.Summary,
                $"{Grading.Format(q.Score)} з {Grading.Format(q.MaxScore)}");

            DataGridViewCell scoreCell = dgvQuestions.Rows[row].Cells[colScore.Index];
            scoreCell.Style.ForeColor = ScoreColor(q);
            scoreCell.Style.Font = boldFont;
        }

        dgvQuestions.ClearSelection();
    }

    // Доброчесність: підсумок, налаштування і хронологія подій контрольної
    private void FillIntegrity(TestResult result)
    {
        const int indent = 18;
        IntegrityReport report = result.Integrity!;
        bool violations = IntegrityText.HasViolations(result);

        RichTextFormatter.AppendParagraph(rtbReview, "Режим доброчесності: " + IntegrityText.Summary(result),
            violations ? Theme.Violation : Theme.Primary, bold: true);
        RichTextFormatter.AppendParagraph(rtbReview, IntegrityText.SettingsText(report), Theme.MutedText, indent: indent);

        foreach (string line in IntegrityText.Timeline(result))
        {
            RichTextFormatter.AppendParagraph(rtbReview, line, rtbReview.ForeColor, indent: indent, bullet: true);
        }

        // Час на кожне питання (якщо записано)
        if (result.Questions.Any(q => q.Seconds > 0))
        {
            string times = string.Join(", ", result.Questions.Select((q, i) => $"П{i + 1} — {IntegrityText.Duration(q.Seconds)}"));
            RichTextFormatter.AppendParagraph(rtbReview, "Час на питання: " + times, Theme.MutedText, indent: indent);
        }

        RichTextFormatter.AppendGap(rtbReview);
        RichTextFormatter.AppendGap(rtbReview);
    }

    // Розбір: для кожного питання — бали, помилки і пояснення (з виділеним кодом)
    private void FillReview(TestResult result)
    {
        const int indent = 18;   // відступ вмісту під заголовком питання

        for (int i = 0; i < result.Questions.Count; i++)
        {
            QuestionResult q = result.Questions[i];

            if (i > 0)
            {
                RichTextFormatter.AppendGap(rtbReview);
                RichTextFormatter.AppendGap(rtbReview);
            }

            RichTextFormatter.AppendParagraph(rtbReview,
                $"Питання {i + 1}. {q.Type.ToUkrainian()} — «{q.Title}»: " +
                $"{Grading.Format(q.Score)} з {Grading.Format(q.MaxScore)} балів",
                Theme.Primary, bold: true);

            if (q.Mistakes.Count == 0)
            {
                RichTextFormatter.AppendParagraph(rtbReview, "✔ Усе правильно", Theme.Success, indent: indent);
            }
            else
            {
                foreach (string mistake in q.Mistakes)
                {
                    RichTextFormatter.AppendParagraph(rtbReview, "✗ " + mistake, Theme.Error, indent: indent, hanging: 14);
                }
            }

            RichTextFormatter.AppendMarkup(rtbReview, q.Explanation, indent);
        }
    }

    private static Color ScoreColor(QuestionResult q)
    {
        if (q.Score >= q.MaxScore)
        {
            return Theme.Success;
        }

        return q.Score > 0 ? Theme.Warning : Theme.Error;
    }
}

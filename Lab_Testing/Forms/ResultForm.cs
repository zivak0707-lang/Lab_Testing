namespace Lab_Testing;

// Сторінка результату: бали за кожне питання, оцінка за 12-бальною шкалою, розбір помилок
public partial class ResultForm : Form
{
    private readonly Font boldFont;   // жирний шрифт для заголовків у розборі

    // showReview = false — розбір помилок вимкнено викладачем (бали й оцінку видно завжди)
    public ResultForm(TestResult result, bool showReview)
    {
        InitializeComponent();
        boldFont = new Font(rtbReview.Font, FontStyle.Bold);

        lblStudent.Text = $"{result.StudentName}  ·  група {result.Group}  ·  режим «{result.Mode.ToUkrainian()}»";
        lblDetails.Text = $"{result.FinishedAt:dd.MM.yyyy HH:mm}  ·  час проходження: {Grading.FormatDuration(result.Duration)}" +
                          (result.FinishedByTimeout ? "  ·  завершено, бо скінчився час" : "");

        lblGrade.Text = result.Grade.ToString();
        lblLevel.Text = Grading.Level(result.Grade);
        lblScore.Text = $"{Grading.Format(result.TotalScore)} з {Grading.Format(result.MaxScore)} балів  ·  " +
                        $"{Grading.Format(result.Percent)} %";

        lblFileInfo.Text = result.Mode == TestMode.Training
            ? "Режим «Навчання»: результат не зберігається у файл."
            : "";

        FillTable(result);

        if (showReview)
        {
            FillReview(result);
        }
        else
        {
            RichTextFormatter.ShowHint(rtbReview,
                "Розбір помилок після контрольної вимкнено викладачем. Ваші бали та оцінку показано вище.");
        }

        // Таблиця сама виділяє перший рядок, коли отримує фокус, — знімаємо виділення
        Shown += (_, _) =>
        {
            dgvQuestions.CurrentCell = null;
            dgvQuestions.ClearSelection();
            btnClose.Focus();
        };

        FormClosed += (_, _) => boldFont.Dispose();
    }

    // Текст про файл результату внизу сторінки (для контрольної — шлях до збереженого файлу)
    public void SetFileInfo(string text)
    {
        lblFileInfo.Text = text;
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

    // Розбір: для кожного питання — бали, помилки і пояснення (з виділеним кодом)
    private void FillReview(TestResult result)
    {
        const int indent = 18;   // відступ вмісту під заголовком питання
        rtbReview.Clear();

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

        rtbReview.SelectionStart = 0;   // прокрутити на початок
        rtbReview.ScrollToCaret();
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

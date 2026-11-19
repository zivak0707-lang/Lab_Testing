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

        FillTable(result);

        if (showReview)
        {
            FillReview(result);
        }
        else
        {
            rtbReview.Text = "Розбір помилок після контрольної вимкнено викладачем. Ваші бали та оцінку показано вище.";
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

    // Розбір: для кожного питання — бали, помилки і пояснення
    private void FillReview(TestResult result)
    {
        for (int i = 0; i < result.Questions.Count; i++)
        {
            QuestionResult q = result.Questions[i];

            Append($"Питання {i + 1}. {q.Type.ToUkrainian()} — «{q.Title}»: " +
                   $"{Grading.Format(q.Score)} з {Grading.Format(q.MaxScore)} балів\n", Theme.Primary, bold: true);

            if (q.Mistakes.Count == 0)
            {
                Append("    ✔ Усе правильно\n", Theme.Success, bold: false);
            }
            else
            {
                foreach (string mistake in q.Mistakes)
                {
                    Append($"    ✗ {mistake}\n", Theme.Error, bold: false);
                }
            }

            Append($"    Пояснення: {q.Explanation}\n\n", Theme.MutedText, bold: false);
        }

        rtbReview.SelectionStart = 0;   // прокрутити на початок
    }

    // Додає в розбір текст заданого кольору
    private void Append(string text, Color color, bool bold)
    {
        rtbReview.SelectionStart = rtbReview.TextLength;
        rtbReview.SelectionLength = 0;
        rtbReview.SelectionColor = color;
        rtbReview.SelectionFont = bold ? boldFont : rtbReview.Font;
        rtbReview.AppendText(text);
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

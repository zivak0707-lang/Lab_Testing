namespace Lab_Testing;

// Вікно проходження тесту: питання по одному, перехід між ними, перевірка (режим «Навчання»)
public partial class TestForm : Form
{
    private readonly TestSession session;
    private readonly List<QuestionView> views = new();     // по поданню на кожне питання (зберігають відповіді)
    private readonly List<Button> stepButtons = new();      // кнопки 1..5 у шапці
    private readonly QuestionResult?[] lastChecks;          // остання перевірка кожного питання (null — не перевірялось)
    private int currentIndex;                                // поточне питання (від 0)
    private bool finished;                                   // тест завершено — закривати без запитань

    public TestForm(TestSession session)
    {
        InitializeComponent();
        this.session = session;
        lastChecks = new QuestionResult?[session.Questions.Count];

        lblStudent.Text = $"{session.StudentName}  ·  група {session.Group}  ·  режим «{session.Mode.ToUkrainian()}»";

        // Теорія, перевірка і скидання — лише в режимі «Навчання»
        bool training = session.Mode == TestMode.Training;
        pnlTheory.Visible = training;
        btnCheck.Visible = training;
        btnReset.Visible = training;

        // Подання для всіх питань створюємо одразу: при переході між питаннями відповіді зберігаються
        foreach (Question question in session.Questions)
        {
            views.Add(CreateView(question));
        }

        CreateStepButtons();
        ShowQuestion(0);
        session.StartedAt = DateTime.Now;
    }

    // Підсумок тесту (з'являється після завершення; null — учень вийшов, не завершивши)
    public TestResult? Result { get; private set; }

    // Подання поточного питання
    public QuestionView CurrentView => views[currentIndex];

    private QuestionView CreateView(Question question)
    {
        QuestionView view = QuestionView.Create(question, session.Random);
        view.AnswerChanged += View_AnswerChanged;
        return view;
    }

    // ===================== Перехід між питаннями =====================

    // Показує питання з номером index
    public void ShowQuestion(int index)
    {
        currentIndex = index;
        Question question = session.Questions[index];

        pnlContent.Controls.Clear();
        pnlContent.Controls.Add(views[index]);

        lblQuestionInfo.Text = $"Питання {index + 1} з {views.Count}  ·  {question.Type.ToUkrainian()}  ·  {question.Topic}";
        lblTask.Text = question.Task;
        lblTheory.Text = question.Theory;
        ShowFeedback(lastChecks[index]);

        btnPrev.Enabled = index > 0;
        btnNext.Enabled = index < views.Count - 1;
        UpdateStepButtons();
    }

    private void BtnPrev_Click(object? sender, EventArgs e)
    {
        ShowQuestion(currentIndex - 1);
    }

    private void BtnNext_Click(object? sender, EventArgs e)
    {
        ShowQuestion(currentIndex + 1);
    }

    // Кнопки з номерами питань у шапці: перехід до будь-якого питання
    private void CreateStepButtons()
    {
        for (int i = 0; i < views.Count; i++)
        {
            Button button = new()
            {
                Text = (i + 1).ToString(),
                Tag = i,
                Size = new Size(LogicalToDeviceUnits(36), LogicalToDeviceUnits(36)),
                Margin = new Padding(LogicalToDeviceUnits(2)),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.SectionFont,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = Color.White;
            button.Click += StepButton_Click;

            stepButtons.Add(button);
            flpSteps.Controls.Add(button);
        }
    }

    private void StepButton_Click(object? sender, EventArgs e)
    {
        if (sender is Button { Tag: int index })
        {
            ShowQuestion(index);
        }
    }

    // Поточне питання — біла кнопка, питання з відповіддю — блакитна, інші — прозорі
    private void UpdateStepButtons()
    {
        for (int i = 0; i < stepButtons.Count; i++)
        {
            Button button = stepButtons[i];

            if (i == currentIndex)
            {
                button.BackColor = Theme.StepCurrent;
                button.ForeColor = Theme.Primary;
            }
            else
            {
                button.BackColor = views[i].HasAnswer ? Theme.StepAnswered : Theme.Primary;
                button.ForeColor = Color.White;
            }
        }
    }

    // Учень переклав картку: стара перевірка вже не актуальна — прибираємо підсвітку
    private void View_AnswerChanged(object? sender, EventArgs e)
    {
        int index = views.IndexOf((QuestionView)sender!);

        if (index >= 0 && lastChecks[index] != null)
        {
            views[index].ClearCheck();
            lastChecks[index] = null;

            if (index == currentIndex)
            {
                ShowFeedback(null);
            }
        }

        UpdateStepButtons();
    }

    // ===================== Режим «Навчання»: перевірка і скидання =====================

    // «Перевірити»: підсвічуємо картки і показуємо бали та пояснення. Спроб скільки завгодно
    private void BtnCheck_Click(object? sender, EventArgs e)
    {
        QuestionResult result = CurrentView.Evaluate(highlight: true);
        lastChecks[currentIndex] = result;
        ShowFeedback(result);
    }

    // «Скинути»: питання починається заново (картки перемішуються ще раз)
    private void BtnReset_Click(object? sender, EventArgs e)
    {
        QuestionView old = views[currentIndex];
        views[currentIndex] = CreateView(old.Question);
        lastChecks[currentIndex] = null;

        ShowQuestion(currentIndex);
        old.Dispose();
    }

    // Показує на панелі теорії результат перевірки (null — сховати)
    private void ShowFeedback(QuestionResult? result)
    {
        bool show = result != null;
        lblCheckTitle.Visible = show;
        lblCheckSummary.Visible = show;
        lblExplanation.Visible = show;

        if (result == null)
        {
            return;
        }

        string score = $"{Grading.Format(result.Score)} з {Grading.Format(result.MaxScore)} балів";

        if (result.Mistakes.Count == 0)
        {
            lblCheckSummary.Text = $"✔ Усе правильно! {score}.";
            lblCheckSummary.ForeColor = Theme.Success;
            lblExplanation.Text = "Пояснення: " + result.Explanation;
        }
        else
        {
            lblCheckSummary.Text = $"{Capitalize(result.Summary)} — {score}.";
            lblCheckSummary.ForeColor = result.Score > 0 ? Theme.Warning : Theme.Error;
            lblExplanation.Text = "Зелені картки — правильно, червоні — помилки, червона рамка — не заповнено. " +
                                  "Виправте й перевірте ще раз.\n\nПояснення: " + result.Explanation;
        }
    }

    private static string Capitalize(string text)
    {
        return text.Length == 0 ? text : char.ToUpper(text[0]) + text[1..];
    }

    // ===================== Завершення =====================

    private void BtnFinish_Click(object? sender, EventArgs e)
    {
        int unanswered = views.Count(v => !v.HasAnswer);
        string warning = unanswered > 0 ? $"Питань без відповіді: {unanswered}.\n\n" : "";

        DialogResult answer = MessageBox.Show(this, warning + "Завершити тест і подивитися результат?", "Завершення тесту",
            MessageBoxButtons.YesNo, unanswered > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            Finish(byTimeout: false);
        }
    }

    // Перевіряє всі питання, складає підсумок і закриває вікно
    private void Finish(bool byTimeout)
    {
        List<QuestionResult> results = views.Select(v => v.Evaluate(highlight: false)).ToList();
        Result = TestResult.Create(session, results, DateTime.Now, byTimeout);

        finished = true;
        DialogResult = DialogResult.OK;
        Close();
    }

    // Закриття вікна хрестиком до завершення тесту — запитуємо підтвердження
    private void TestForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (finished || e.CloseReason != CloseReason.UserClosing)
        {
            return;
        }

        DialogResult answer = MessageBox.Show(this, "Вийти з тесту? Відповіді не буде збережено.", "Вихід",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        e.Cancel = answer != DialogResult.Yes;
    }

    // Звільняємо подання питань, які зараз не показані у вікні
    private void TestForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        foreach (QuestionView view in views)
        {
            view.Dispose();
        }
    }
}

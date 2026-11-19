namespace Lab_Testing;

// Вікно проходження тесту: питання по одному, перехід між ними
public partial class TestForm : Form
{
    private readonly TestSession session;
    private readonly List<QuestionView> views = new();     // по поданню на кожне питання (зберігають відповіді)
    private readonly List<Button> stepButtons = new();      // кнопки 1..5 у шапці
    private int currentIndex;                                // поточне питання (від 0)
    private bool finished;                                   // тест завершено — закривати без запитань

    public TestForm(TestSession session)
    {
        InitializeComponent();
        this.session = session;

        lblStudent.Text = $"{session.StudentName}  ·  група {session.Group}  ·  режим «{session.Mode.ToUkrainian()}»";

        // Подання для всіх питань створюємо одразу: при переході між питаннями відповіді зберігаються
        foreach (Question question in session.Questions)
        {
            QuestionView view = QuestionView.Create(question, session.Random);
            view.AnswerChanged += View_AnswerChanged;
            views.Add(view);
        }

        CreateStepButtons();
        ShowQuestion(0);
    }

    // Подання поточного питання
    public QuestionView CurrentView => views[currentIndex];

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

    private void View_AnswerChanged(object? sender, EventArgs e)
    {
        UpdateStepButtons();
    }

    // ===================== Завершення =====================

    private void BtnFinish_Click(object? sender, EventArgs e)
    {
        int unanswered = views.Count(v => !v.HasAnswer);
        string warning = unanswered > 0 ? $"Питань без відповіді: {unanswered}.\n\n" : "";

        DialogResult answer = MessageBox.Show(this, warning + "Завершити тест?", "Завершення тесту",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            // Підрахунок балів і сторінка результату з'являться на етапі 4
            finished = true;
            Close();
        }
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

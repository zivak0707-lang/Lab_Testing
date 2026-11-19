namespace Lab_Testing;

// Вікно проходження тесту: питання по одному, перехід між ними, перевірка (режим «Навчання»)
public partial class TestForm : Form
{
    private readonly TestSession session;
    private readonly List<QuestionView> views = new();     // по поданню на кожне питання (зберігають відповіді)
    private readonly List<Button> stepButtons = new();      // кнопки 1..5 у шапці
    private readonly QuestionResult?[] lastChecks;          // остання перевірка кожного питання (null — не перевірялось)
    private readonly int[] checkCounts;                      // скільки разів перевіряли змінену відповідь
    private readonly bool[] answerShown;                     // учень відкрив правильну відповідь
    private int currentIndex;                                // поточне питання (від 0)
    private bool finished;                                   // тест завершено — закривати без запитань

    public TestForm(TestSession session)
    {
        InitializeComponent();
        this.session = session;
        lastChecks = new QuestionResult?[session.Questions.Count];
        checkCounts = new int[session.Questions.Count];
        answerShown = new bool[session.Questions.Count];

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

        // Контрольна: таймер зворотного відліку
        if (!training)
        {
            btnFinish.Text = "Завершити контрольну";
            lblTimer.Visible = true;
            UpdateTimer();
            controlTimer.Start();
        }
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
        RichTextFormatter.Show(rtbTheory, question.Theory);
        ShowFeedback(lastChecks[index]);
        tabInfo.SelectedTab = lastChecks[index] != null ? tabCheck : tabTheory;

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

    // ===================== Режим «Навчання»: перевірка, відповідь, скидання =====================

    // Після скількох перевірок (зі зміненою відповіддю) можна відкрити правильну відповідь
    private const int ChecksBeforeAnswer = 2;

    // «Перевірити»: підсвічуємо картки, показуємо бали і що виправити — але не саму відповідь
    private void BtnCheck_Click(object? sender, EventArgs e)
    {
        if (!CurrentView.HasAnswer)
        {
            ShowNotAnswered();
            return;
        }

        // Спробою вважається лише перевірка зміненої відповіді (повторне натискання без змін не рахується)
        if (lastChecks[currentIndex] == null)
        {
            checkCounts[currentIndex]++;
        }

        lastChecks[currentIndex] = CurrentView.Evaluate(highlight: true);
        ShowFeedback(lastChecks[currentIndex]);
        tabInfo.SelectedTab = tabCheck;
    }

    // «Показати правильну відповідь»: учень відкриває її свідомо, після кількох спроб
    private void BtnShowAnswer_Click(object? sender, EventArgs e)
    {
        answerShown[currentIndex] = true;
        ShowFeedback(lastChecks[currentIndex]);
        tabInfo.SelectedTab = tabCheck;
    }

    // «Скинути»: питання починається заново (картки перемішуються ще раз); спроби не обнуляються
    private void BtnReset_Click(object? sender, EventArgs e)
    {
        QuestionView old = views[currentIndex];
        views[currentIndex] = CreateView(old.Question);
        lastChecks[currentIndex] = null;

        ShowQuestion(currentIndex);
        old.Dispose();
    }

    // Перевірити нічого: учень ще не поклав жодної картки
    private void ShowNotAnswered()
    {
        pnlCheck.Visible = true;
        pnlCheckGap.Visible = true;
        pnlCheck.BackColor = Theme.SelectedBack;
        lblCheckSummary.Text = "Спершу дайте відповідь";
        lblCheckSummary.ForeColor = Theme.Primary;
        lblCheckHint.Text = "Перетягніть картки, а потім натисніть «Перевірити». Порожню відповідь не перевіряємо.";
        FitCheckPanel();
    }

    // Показує результат перевірки над вкладками і вміст вкладки «Перевірка» (result == null — ще не перевірялось)
    private void ShowFeedback(QuestionResult? result)
    {
        Question question = session.Questions[currentIndex];
        bool perfect = result != null && result.Mistakes.Count == 0;
        bool showAnswer = perfect || answerShown[currentIndex];

        pnlCheck.Visible = result != null;
        pnlCheckGap.Visible = result != null;
        rtbCheck.Clear();

        if (result == null)
        {
            RichTextFormatter.AppendParagraph(rtbCheck,
                "Натисніть «Перевірити» — тут з'явиться, що саме треба виправити.", Theme.MutedText);
        }
        else
        {
            string score = $"{Grading.Format(result.Score)} з {Grading.Format(result.MaxScore)} балів";

            if (perfect)
            {
                lblCheckSummary.Text = $"✔ Усе правильно! {score}";
                lblCheckSummary.ForeColor = Theme.Success;
                pnlCheck.BackColor = Theme.CorrectBack;
                lblCheckHint.Text = "Нижче — пояснення, щоб закріпити тему. Можна переходити до наступного питання.";
            }
            else
            {
                bool partly = result.Score > 0;
                lblCheckSummary.Text = $"{Capitalize(result.Summary)} — {score}";
                lblCheckSummary.ForeColor = partly ? Theme.Warning : Theme.Error;
                pnlCheck.BackColor = partly ? Theme.PartlyBack : Theme.WrongBack;
                lblCheckHint.Text = "Зелене — правильно, червоне — помилка. Червона рамка — не заповнено, " +
                                    "помаранчева — тут ще є що виправити.";

                // Що виправити — без готової відповіді
                RichTextFormatter.AppendHeading(rtbCheck, "Що виправити");

                foreach (string issue in result.Issues)
                {
                    RichTextFormatter.AppendParagraph(rtbCheck, issue, rtbCheck.ForeColor, bullet: true);
                }

                if (question.Hint.Length > 0)
                {
                    RichTextFormatter.AppendHeading(rtbCheck, "Підказка");
                    RichTextFormatter.AppendMarkup(rtbCheck, question.Hint);
                }
            }
        }

        // Повна відповідь: якщо все правильно (як пояснення) або якщо учень сам її відкрив
        if (showAnswer)
        {
            RichTextFormatter.AppendMarkup(rtbCheck, question.Explanation);
        }

        rtbCheck.SelectionStart = 0;
        rtbCheck.ScrollToCaret();
        UpdateAnswerButton(perfect);
        FitCheckPanel();
    }

    // Висота блоку результату під довжину текстів (довгий підсумок переноситься на другий рядок)
    private void FitCheckPanel()
    {
        int width = pnlCheck.ClientSize.Width - pnlCheck.Padding.Horizontal;
        lblCheckSummary.Height = TextHeight(lblCheckSummary, width);
        pnlCheck.Height = pnlCheck.Padding.Vertical + lblCheckSummary.Height + TextHeight(lblCheckHint, width);
    }

    private static int TextHeight(Label label, int width)
    {
        Size size = TextRenderer.MeasureText(label.Text, label.Font, new Size(width, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        return size.Height + 6;
    }

    // Кнопка «Показати правильну відповідь»: доступна після ChecksBeforeAnswer спроб
    private void UpdateAnswerButton(bool perfect)
    {
        int checks = checkCounts[currentIndex];

        if (perfect || answerShown[currentIndex])
        {
            btnShowAnswer.Enabled = false;
            btnShowAnswer.Text = perfect ? "Відповідь правильна" : "Правильну відповідь показано";
        }
        else if (checks >= ChecksBeforeAnswer)
        {
            btnShowAnswer.Enabled = true;
            btnShowAnswer.Text = "Показати правильну відповідь";
        }
        else
        {
            btnShowAnswer.Enabled = false;
            btnShowAnswer.Text = $"Відповідь — після {ChecksBeforeAnswer} спроб (зараз {checks})";
        }
    }

    private static string Capitalize(string text)
    {
        return text.Length == 0 ? text : char.ToUpper(text[0]) + text[1..];
    }

    // ===================== Завершення =====================

    private bool IsControl => session.Mode == TestMode.Control;

    private void BtnFinish_Click(object? sender, EventArgs e)
    {
        if (AskFinish())
        {
            Finish(byTimeout: false);
        }
    }

    // Запитує, чи завершити тест; попереджає про питання без відповіді
    private bool AskFinish()
    {
        int unanswered = views.Count(v => !v.HasAnswer);
        string warning = unanswered > 0 ? $"Питань без відповіді: {unanswered}.\n\n" : "";
        string question = IsControl
            ? "Завершити контрольну? Після завершення змінити відповіді не можна, а результат буде збережено у файл."
            : "Завершити тест і подивитися результат?";

        return Dialogs.Ask(this, warning + question, IsControl ? "Завершення контрольної" : "Завершення тесту",
            warning: unanswered > 0);
    }

    // ===================== Таймер контрольної =====================

    // Щосекунди оновлюємо залишок часу
    private void ControlTimer_Tick(object? sender, EventArgs e)
    {
        UpdateTimer();
    }

    // Залишок рахуємо від часу початку (а не кількості тиків), тому таймер не «пливе»
    private void UpdateTimer()
    {
        TimeSpan left = session.StartedAt.AddMinutes(session.Settings.ControlTimeMinutes) - DateTime.Now;

        if (left <= TimeSpan.Zero)
        {
            lblTimer.Text = "Залишилось 00:00";
            TimeIsUp();
            return;
        }

        lblTimer.Text = $"Залишилось {(int)left.TotalMinutes:00}:{left.Seconds:00}";
        lblTimer.ForeColor = left.TotalSeconds <= 120 ? Theme.TimerWarning : Color.White;   // останні 2 хвилини — червоний
    }

    // Час вийшов: відповіді фіксуємо одразу, а потім повідомляємо учня
    private void TimeIsUp()
    {
        controlTimer.Stop();
        PrepareResult(byTimeout: true);

        Dialogs.Info(this, "Час вийшов! Контрольну завершено автоматично.\n\n" +
                           "Враховано всі відповіді, які ви встигли дати. Результат буде збережено у файл.",
            "Час вийшов");

        DialogResult = DialogResult.OK;
        Close();
    }

    // ===================== Підсумок =====================

    // Перевіряє всі питання, складає підсумок і закриває вікно
    private void Finish(bool byTimeout)
    {
        PrepareResult(byTimeout);
        DialogResult = DialogResult.OK;
        Close();
    }

    // Перевіряє всі питання і складає підсумок (після цього тест завершено — змінити відповіді не можна)
    private void PrepareResult(bool byTimeout)
    {
        controlTimer.Stop();
        List<QuestionResult> results = views.Select(v => v.Evaluate(highlight: false)).ToList();

        // У «Навчанні» позначаємо питання, де учень відкривав правильну відповідь
        for (int i = 0; i < results.Count; i++)
        {
            if (answerShown[i])
            {
                results[i].Summary += " · відповідь переглянуто";
            }
        }

        Result = TestResult.Create(session, results, DateTime.Now, byTimeout);
        finished = true;
    }

    // Закриття вікна хрестиком до завершення тесту.
    // «Навчання» — просто вихід. «Контрольна» — закрити вікно означає завершити контрольну:
    // результат зберігається, повторно пройти цю спробу не можна
    private void TestForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (finished)
        {
            return;
        }

        if (IsControl)
        {
            if (e.CloseReason != CloseReason.UserClosing || AskFinish())
            {
                PrepareResult(byTimeout: false);
                DialogResult = DialogResult.OK;
            }
            else
            {
                e.Cancel = true;
            }

            return;
        }

        if (e.CloseReason != CloseReason.UserClosing)
        {
            return;
        }

        e.Cancel = !Dialogs.Ask(this, "Вийти з тесту? Відповіді не буде збережено.", "Вихід", warning: true);
    }

    // Звільняємо подання питань, які зараз не показані у вікні
    private void TestForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        controlTimer.Stop();

        foreach (QuestionView view in views)
        {
            view.Dispose();
        }
    }
}

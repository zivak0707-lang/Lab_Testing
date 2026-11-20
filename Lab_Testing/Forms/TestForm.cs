using Microsoft.Win32;

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

    // Час на кожному питанні (для вчителя: підозріло швидкі відповіді)
    private readonly double[] questionSeconds;
    private DateTime? questionSince;                         // відколи учень у вікні на поточному питанні (null — поза вікном)

    // Режим доброчесності контрольної (null — вимкнено або це «Навчання»)
    private readonly IntegrityMonitor? integrity;
    private bool monitoring;                                 // виходи відстежуються (з моменту показу вікна)
    private IDisposable? controlMarker;                      // позначка «іде контрольна» для інших копій програми
    private int monitorCount;                                // скільки моніторів зараз
    private DateTime noticeUntil;                            // до якого часу у смузі вгорі показано повідомлення
    private bool warningShown;                               // зараз відкрито попередження про вихід
    private bool appActive = true;                           // учень зараз у програмі (не в іншій)

    public TestForm(TestSession session)
    {
        InitializeComponent();
        this.session = session;
        lastChecks = new QuestionResult?[session.Questions.Count];
        checkCounts = new int[session.Questions.Count];
        answerShown = new bool[session.Questions.Count];
        questionSeconds = new double[session.Questions.Count];

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
        questionSince = session.StartedAt;

        // Контрольна: таймер зворотного відліку
        if (!training)
        {
            btnFinish.Text = "Завершити контрольну";
            lblTimer.Visible = true;
            UpdateTimer();
            controlTimer.Start();
        }

        // Режим доброчесності: вікно на весь екран поверх інших (до створення вікна — так Windows не перестворює його)
        if (!training && session.Settings.Integrity.Enabled)
        {
            integrity = new IntegrityMonitor(session.Settings.Integrity);

            if (session.RulesAcceptedAt is DateTime accepted)
            {
                integrity.Add(IntegrityEventKind.RulesAccepted, accepted);
            }

            EnterFullScreen();
        }
    }

    // Підсумок тесту (з'являється після завершення; null — учень вийшов, не завершивши)
    public TestResult? Result { get; private set; }

    // Що записав режим доброчесності (null — вимкнено); для самоперевірок
    public IntegrityReport? IntegrityReport => integrity?.Report;

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
        AddQuestionTime(DateTime.Now);   // час іде попередньому питанню
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
            Finish(FinishReason.Student);
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

        // Повідомлення у смузі вгорі показуємо кілька секунд, потім — знову стан виходів
        if (integrity != null && noticeUntil != DateTime.MinValue && DateTime.Now >= noticeUntil)
        {
            noticeUntil = DateTime.MinValue;
            UpdateIntegrityBar();
        }
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
        PrepareResult(FinishReason.TimeUp);

        Dialogs.Info(this, "Час вийшов! Контрольну завершено автоматично.\n\n" +
                           "Враховано всі відповіді, які ви встигли дати. Результат буде збережено у файл.",
            "Час вийшов");

        DialogResult = DialogResult.OK;
        Close();
    }

    // ===================== Підсумок =====================

    // Перевіряє всі питання, складає підсумок і закриває вікно
    private void Finish(FinishReason reason)
    {
        PrepareResult(reason);
        DialogResult = DialogResult.OK;
        Close();
    }

    // Перевіряє всі питання і складає підсумок (після цього тест завершено — змінити відповіді не можна)
    private void PrepareResult(FinishReason reason)
    {
        controlTimer.Stop();
        DateTime now = DateTime.Now;
        AddQuestionTime(now);
        questionSince = null;

        // Якщо учень саме поза вікном — вихід триває до кінця контрольної
        integrity?.Close(now);
        monitoring = false;

        List<QuestionResult> results = views.Select(v => v.Evaluate(highlight: false)).ToList();

        for (int i = 0; i < results.Count; i++)
        {
            // У «Навчанні» позначаємо питання, де учень відкривав правильну відповідь
            if (answerShown[i])
            {
                results[i].Summary += " · відповідь переглянуто";
            }

            // У контрольній — скільки часу учень провів на питанні
            if (IsControl)
            {
                results[i].Seconds = (int)Math.Round(questionSeconds[i]);
            }
        }

        Result = TestResult.Create(session, results, now, reason, integrity?.Report);
        finished = true;
    }

    // Час на поточному питанні: додаємо відрізок від questionSince до now (лише коли учень у вікні)
    private void AddQuestionTime(DateTime now)
    {
        if (questionSince is DateTime since)
        {
            questionSeconds[currentIndex] += (now - since).TotalSeconds;
            questionSince = now;
        }
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
                PrepareResult(FinishReason.WindowClosed);
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

    // Звільняємо подання питань, які зараз не показані у вікні, і припиняємо стежити за виходами
    private void TestForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        controlTimer.Stop();
        StopIntegrity();

        foreach (QuestionView view in views)
        {
            view.Dispose();
        }
    }

    // ===================== Режим доброчесності (лише «Контрольна») =====================
    // Нічого не блокує: Alt+Tab, Win, Ctrl+Alt+Del і Alt+F4 працюють як завжди.
    // Програма лише помічає, що активним стало вікно іншої програми, і записує це

    // Контрольна на весь екран (разом з панеллю задач). «Поверх інших вікон» вмикається, коли вікно показано,
    // і лише поки учень у тесті (див. WndProc)
    private void EnterFullScreen()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.FromPoint(Cursor.Position).Bounds;
        BackColor = Theme.ExamBackdrop;
        pnlExamBar.Visible = true;
        UpdateIntegrityBar();
    }

    // Вміст тесту — посередині екрана у звичному розмірі, вгорі — смуга режиму доброчесності
    private void TestForm_Resize(object? sender, EventArgs e)
    {
        if (integrity == null || WindowState == FormWindowState.Minimized || ClientSize.Height == 0)
        {
            return;
        }

        Size content = new(LogicalToDeviceUnits(1180), LogicalToDeviceUnits(680));
        int bar = pnlExamBar.Height;
        int side = Math.Max(0, (ClientSize.Width - content.Width) / 2);
        int top = Math.Max(bar + LogicalToDeviceUnits(6), (ClientSize.Height - content.Height) / 2);
        int bottom = Math.Max(0, ClientSize.Height - top - content.Height);

        Padding = new Padding(side, top, side, bottom);
        pnlExamBar.SetBounds(0, 0, ClientSize.Width, bar);
    }

    // Вікно показано: починаємо відстежувати виходи, знімки екрана, монітори і другу копію програми
    private void TestForm_Shown(object? sender, EventArgs e)
    {
        if (integrity == null)
        {
            return;
        }

        IntegrityReport report = integrity.Report;
        report.Monitors = monitorCount = Screen.AllScreens.Length;
        report.RemoteSession = WindowsApi.IsRemoteSession;
        report.CaptureProtection = integrity.Settings.HideFromCapture ? WindowsApi.HideFromCapture(Handle) : "вимкнено";

        KeyPreview = true;   // щоб помітити клавішу Print Screen, хоч би на якій картці був фокус
        WindowsApi.WatchClipboard(Handle, true);
        SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        controlMarker = InstanceGuard.BeginControl(() => RunOnFormThread(SecondCopyAttempted));

        monitoring = true;
        // Поверх інших вікон — якщо учень у тесті (якщо він уже в іншій програмі, тест її не перекриває)
        if (ActiveForm == this)
        {
            WindowsApi.KeepOnTop(Handle);
        }

        UpdateIntegrityBar();
    }

    private void StopIntegrity()
    {
        monitoring = false;
        controlMarker?.Dispose();
        controlMarker = null;

        if (integrity != null)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            WindowsApi.WatchClipboard(Handle, false);
        }
    }

    // Повідомлення Windows: активною стала інша програма / знову наша; змінився буфер обміну
    protected override void WndProc(ref Message m)
    {
        // «Поверх усіх» — лише поки учень у тесті. Перейшов в іншу програму — тест відступає під її вікно
        // (вона видна і працює), повернувся — тест знову поверх. Фокус програма ніколи не забирає сама
        if (integrity != null && !finished && m.Msg == WindowsApi.WM_ACTIVATEAPP)
        {
            if (m.WParam == IntPtr.Zero)
            {
                appActive = false;

                // Коли інше вікно вже стане активним (і якщо учень за цей час не повернувся)
                BeginInvoke(new Action(() =>
                {
                    if (!appActive)
                    {
                        WindowsApi.StepAside(Handle);
                    }
                }));
            }
            else
            {
                appActive = true;
                WindowsApi.KeepOnTop(Handle);   // до попередження — щоб і воно було поверх
            }
        }

        if (monitoring && !finished)
        {
            if (m.Msg == WindowsApi.WM_ACTIVATEAPP)
            {
                if (m.WParam == IntPtr.Zero)
                {
                    StudentLeft();
                }
                else
                {
                    BeginInvoke(new Action(StudentReturned));
                }
            }
            else if (m.Msg == WindowsApi.WM_CLIPBOARDUPDATE)
            {
                BeginInvoke(new Action(ClipboardChanged));
            }
        }

        base.WndProc(ref m);
    }

    // Учень перейшов в іншу програму (Alt+Tab, клік в інше вікно, згортання). Власні діалоги тесту виходом не є
    private void StudentLeft()
    {
        DateTime now = DateTime.Now;
        AddQuestionTime(now);
        questionSince = null;
        integrity!.Leave(now, currentIndex + 1);
    }

    // Учень повернувся: записуємо вихід і показуємо, що його записано
    private void StudentReturned()
    {
        if (finished || integrity == null || !integrity.IsAway)
        {
            return;
        }

        DateTime now = DateTime.Now;
        IntegrityEvent? exit = integrity.Return(now);
        questionSince = now;

        if (exit == null)
        {
            return;
        }

        UpdateIntegrityBar();

        if (exit.Kind == IntegrityEventKind.ShortExit)
        {
            ShowNotice($"Коротку втрату фокусу ({exit.Seconds} с) записано — вона не рахується як вихід.");
            return;
        }

        if (integrity.LimitReached)
        {
            FinishEarly();
            return;
        }

        // Одне попередження за раз: якщо учень вийде, поки воно відкрите, — лише оновимо смугу вгорі
        if (!warningShown)
        {
            warningShown = true;
            Dialogs.Warning(this, ExitWarningText(exit), "Вихід з вікна тесту записано");
            warningShown = false;
        }
    }

    private string ExitWarningText(IntegrityEvent exit)
    {
        int limit = integrity!.Settings.ExitLimit;
        int exits = integrity.Report.Exits;

        string rule = limit == 0
            ? "Кількість виходів не обмежено, але кожен вихід видно вчителю."
            : integrity.ExitsLeft == 1
                ? $"Зараховано виходів: {exits} з {limit}. Наступний вихід завершить контрольну достроково."
                : $"Зараховано виходів: {exits} з {limit}. Після {limit}-го виходу контрольну буде завершено достроково " +
                  "з тими відповідями, які ви встигли дати.";

        return $"Ви вийшли з вікна контрольної на {IntegrityText.Duration(exit.Seconds)} (питання {exit.Question}).\n\n" +
               "Цей вихід записано у файл результату — вчитель його побачить.\n\n" + rule;
    }

    // Ліміт виходів вичерпано: контрольна завершується з поточними відповідями
    private void FinishEarly()
    {
        // Закриваємо відкриті діалоги тесту (наприклад, «Завершити контрольну?»)
        foreach (Form owned in OwnedForms)
        {
            owned.Close();
        }

        int exits = integrity!.Report.Exits;
        integrity.Add(IntegrityEventKind.EarlyFinish, DateTime.Now, currentIndex + 1, $"зараховано виходів: {exits}");
        PrepareResult(FinishReason.ExitLimit);

        Dialogs.Warning(this,
            $"Контрольну завершено достроково: ви вийшли з вікна тесту {IntegrityText.Times(exits)} — " +
            "це ліміт, встановлений вчителем.\n\n" +
            "Враховано всі відповіді, які ви встигли дати. Результат разом із записом усіх виходів буде збережено у файл.",
            "Контрольну завершено достроково");

        DialogResult = DialogResult.OK;
        Close();
    }

    // Знімок екрана: клавіша Print Screen у вікні тесту або зображення, що потрапило в буфер обміну.
    // Вміст буфера не читаємо — лише перевіряємо, чи там зображення
    private void TestForm_KeyUp(object? sender, KeyEventArgs e)
    {
        if (monitoring && !finished && e.KeyCode == Keys.PrintScreen)
        {
            integrity!.AddScreenshot(DateTime.Now, currentIndex + 1, "клавіша Print Screen");
            ShowNotice("Спробу зробити знімок екрана записано.");
        }
    }

    private void ClipboardChanged()
    {
        if (!monitoring || finished)
        {
            return;
        }

        try
        {
            if (Clipboard.ContainsImage())
            {
                integrity!.AddScreenshot(DateTime.Now, currentIndex + 1, "зображення в буфері обміну");
                ShowNotice("Спробу зробити знімок екрана записано.");
            }
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            // буфер обміну саме зайнятий іншою програмою — пропускаємо
        }
    }

    // Під'єднали чи від'єднали монітор: записуємо і знову розтягуємо тест на весь екран
    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
    {
        RunOnFormThread(() =>
        {
            int count = Screen.AllScreens.Length;

            if (monitoring && count != monitorCount)
            {
                integrity!.Add(IntegrityEventKind.Monitors, DateTime.Now, currentIndex + 1, $"стало {count}");
                monitorCount = count;
            }

            if (!finished && WindowState != FormWindowState.Minimized)
            {
                Bounds = Screen.FromControl(this).Bounds;
            }
        });
    }

    // Під час контрольної до комп'ютера підключились через віддалений робочий стіл
    private void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.RemoteConnect)
        {
            RunOnFormThread(() =>
            {
                if (monitoring)
                {
                    integrity!.Add(IntegrityEventKind.RemoteSession, DateTime.Now, currentIndex + 1, "підключено під час контрольної");
                }
            });
        }
    }

    // Хтось намагався відкрити ще одну копію програми (вона не відкрилася)
    private void SecondCopyAttempted()
    {
        if (monitoring && !finished)
        {
            integrity!.Add(IntegrityEventKind.SecondCopy, DateTime.Now, currentIndex + 1);
            ShowNotice("Спробу відкрити ще одну копію програми записано.");
        }
    }

    // Події Windows приходять з інших потоків — виконуємо дію в потоці вікна
    private void RunOnFormThread(Action action)
    {
        try
        {
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(action);
            }
        }
        catch (InvalidOperationException)
        {
            // вікно саме закривається — подія вже не потрібна
        }
    }

    // «—»: згорнути вікно тесту. Це звичайний вихід з вікна — його буде записано
    private void BtnMinimizeTest_Click(object? sender, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
    }

    // «✕»: як хрестик у звичайному вікні — завершити контрольну (з підтвердженням)
    private void BtnCloseTest_Click(object? sender, EventArgs e)
    {
        Close();
    }

    // Смуга вгорі: що відстежується і скільки виходів уже зараховано
    private void UpdateIntegrityBar()
    {
        if (integrity == null || noticeUntil != DateTime.MinValue)
        {
            return;
        }

        IntegrityReport report = integrity.Report;
        int limit = integrity.Settings.ExitLimit;
        string exits = limit > 0 ? $"зараховано виходів: {report.Exits} з {limit}" : $"записано виходів: {report.Exits}";
        string capture = integrity.Settings.HideFromCapture ? "  ·  вікно сховано від знімків екрана" : "";

        lblIntegrity.Text = $"Режим доброчесності  ·  виходи з вікна тесту записуються  ·  {exits}{capture}";
        lblIntegrity.ForeColor = report.Exits > 0 ? Theme.TimerWarning : Theme.IntegrityText;
    }

    // Повідомлення у смузі вгорі на кілька секунд (короткий вихід, знімок екрана)
    private void ShowNotice(string text)
    {
        noticeUntil = DateTime.Now.AddSeconds(6);
        lblIntegrity.Text = text;
        lblIntegrity.ForeColor = Color.White;
    }
}

namespace Lab_Testing;

// Стартова сторінка: дані учня і вибір режиму
public partial class StartForm : Form
{
    private readonly QuestionBankData bank;   // завантажений банк питань
    private readonly string bankSource;       // звідки взято банк

    public StartForm(QuestionBankData bank, string bankSource)
    {
        InitializeComponent();
        this.bank = bank;
        this.bankSource = bankSource;

        // Тексти, які залежать від банку і версії програми
        lblBankInfo.Text = $"Банк питань: {bank.Questions.Count} питань ({bankSource})";
        lblControlDesc.Text = lblControlDesc.Text.Replace("15 хв", $"{bank.Settings.ControlTimeMinutes} хв");
        lblAuthor.Text = $"Автор: {AppInfo.Author}, група {AppInfo.Group} · версія {AppInfo.Version}";

        UpdateModeCards();
    }

    // ===================== Вибір режиму =====================

    // Радіокнопки лежать на різних панелях, тому взаємне вимкнення робимо вручну
    private void RbMode_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender == rbTraining && rbTraining.Checked)
        {
            rbControl.Checked = false;
        }
        else if (sender == rbControl && rbControl.Checked)
        {
            rbTraining.Checked = false;
        }

        UpdateModeCards();
    }

    // Клік по картці режиму (по панелі або її опису) вибирає цей режим
    private void ModeCard_Click(object? sender, EventArgs e)
    {
        if (sender == pnlTraining || sender == lblTrainingDesc)
        {
            rbTraining.Checked = true;
        }
        else
        {
            rbControl.Checked = true;
        }
    }

    // Рамка картки режиму: товста синя у вибраної, тонка сіра в іншої
    private void ModeCard_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel panel)
        {
            return;
        }

        bool selected = (panel == pnlTraining && rbTraining.Checked) || (panel == pnlControl && rbControl.Checked);
        int width = selected ? 3 : 1;

        using Pen pen = new(selected ? Theme.Accent : Theme.Border, width);
        Rectangle r = panel.ClientRectangle;
        r.Inflate(-(width + 1) / 2, -(width + 1) / 2);
        e.Graphics.DrawRectangle(pen, r);
    }

    // Оновлює вигляд карток режимів і напис на кнопці
    private void UpdateModeCards()
    {
        pnlTraining.BackColor = rbTraining.Checked ? Theme.SelectedBack : Color.White;
        pnlControl.BackColor = rbControl.Checked ? Theme.SelectedBack : Color.White;
        pnlTraining.Invalidate();
        pnlControl.Invalidate();

        btnStart.Text = rbTraining.Checked ? "Почати навчання" : "Почати контрольну";
    }

    // ===================== Початок тесту =====================

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        string name = NormalizeSpaces(txtName.Text);
        string group = NormalizeSpaces(txtGroup.Text);

        // Перевірка введених даних
        if (name.Length == 0)
        {
            ShowWarning("Введіть прізвище, ім'я та по батькові.");
            txtName.Focus();
            return;
        }

        if (!name.Contains(' '))
        {
            ShowWarning("Введіть принаймні прізвище та ім'я через пробіл.");
            txtName.Focus();
            return;
        }

        if (group.Length == 0)
        {
            ShowWarning("Введіть назву групи.");
            txtGroup.Focus();
            return;
        }

        TestMode mode = rbTraining.Checked ? TestMode.Training : TestMode.Control;
        TestSession session = new(bank, name, group, mode);

        // Контрольна: якщо цей учень уже проходив її на цьому комп'ютері — попереджаємо,
        // що спробу буде позначено як повторну (викладач побачить номер спроби)
        if (mode == TestMode.Control)
        {
            int previous = ResultFile.CountPreviousAttempts(name, group);

            if (previous > 0 && !ConfirmRepeatedAttempt(previous))
            {
                return;
            }

            session.AttemptNumber = previous + 1;

            // Режим доброчесності: інших копій програми бути не повинно, правила треба підтвердити
            if (bank.Settings.Integrity.Enabled && !PrepareIntegrity(session))
            {
                return;
            }
        }

        // Ховаємо стартову сторінку на час тесту і повертаємо її після закриття вікна тесту
        Hide();
        TestResult? result;

        using (TestForm test = new(session))
        {
            test.ShowDialog();
            result = test.Result;   // null — учень вийшов з «Навчання», не завершивши тест
        }

        if (result != null)
        {
            // Розбір помилок: у «Навчанні» завжди, у «Контрольній» — якщо дозволив викладач.
            // Якщо не дозволив — правильних відповідей немає ні на екрані, ні у файлі
            bool showReview = mode == TestMode.Training || bank.Settings.ShowReviewAfterControl;

            if (!showReview)
            {
                result.RemoveCorrectAnswers();
            }

            string? filePath = mode == TestMode.Control ? SaveResultFile(result) : null;

            using ResultForm resultForm = new(result, showReview, filePath);
            resultForm.ShowDialog();
        }

        Show();
    }

    // Перед контрольною в режимі доброчесності: закрити інші копії програми і підтвердити правила
    private bool PrepareIntegrity(TestSession session)
    {
        int others = InstanceGuard.OtherCopies;

        if (others > 0)
        {
            Dialogs.Warning(this,
                $"Перед контрольною закрийте інші вікна програми «{AppInfo.Title}» (відкрито ще: {others}).\n\n" +
                "Під час контрольної інша копія програми працювати не повинна.",
                "Закрийте інші копії програми");
            return false;
        }

        using IntegrityForm rules = new(bank.Settings);

        if (rules.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        session.RulesAcceptedAt = rules.AcceptedAt;
        return true;
    }

    private bool ConfirmRepeatedAttempt(int previous)
    {
        return Dialogs.Ask(this,
            $"Ви вже проходили контрольну на цьому комп'ютері (спроб: {previous}).\n\n" +
            $"Нову спробу буде позначено як повторну (спроба №{previous + 1}) — викладач це побачить.\n\nПочати?",
            "Повторна спроба", warning: true);
    }

    // Зберігає файл результату контрольної і показує учневі, де він лежить (null — зберегти не вдалося)
    private string? SaveResultFile(TestResult result)
    {
        try
        {
            string path = ResultFile.Save(result);

            Dialogs.Info(this,
                $"Результат збережено у файл:\n\n{path}\n\n" +
                "Надішліть цей файл викладачеві (через Classroom, Telegram тощо).\n" +
                "Файл захищено підписом: якщо змінити в ньому хоч щось, викладач це побачить.",
                "Результат збережено");

            return path;
        }
        catch (IOException ex)
        {
            Dialogs.Error(this, "Не вдалося зберегти файл результату.\n\n" + ex.Message +
                                "\n\nПокажіть цей результат викладачеві на екрані.",
                "Помилка збереження");
            return null;
        }
    }

    // ===================== Посилання внизу =====================

    private void LnkAbout_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using AboutForm about = new();
        about.ShowDialog(this);
    }

    // «Режим викладача»: спочатку пароль, потім вікно з результатами учнів
    private void LnkTeacher_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using (PasswordForm password = new())
        {
            if (password.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
        }

        Hide();

        using (TeacherForm teacher = new(bank, bankSource))
        {
            teacher.ShowDialog();
        }

        Show();
    }

    // ===================== Допоміжне =====================

    // Прибирає зайві пробіли: "  Іваненко   Олена " -> "Іваненко Олена"
    private static string NormalizeSpaces(string text)
    {
        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private void ShowWarning(string message)
    {
        Dialogs.Warning(this, message, "Перевірте дані");
    }
}

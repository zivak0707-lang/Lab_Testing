namespace Lab_Testing;

// Стартова сторінка: дані учня і вибір режиму
public partial class StartForm : Form
{
    private readonly QuestionBankData bank;   // завантажений банк питань

    public StartForm(QuestionBankData bank, string bankSource)
    {
        InitializeComponent();
        this.bank = bank;

        // Тексти, які залежать від банку і версії програми
        lblBankInfo.Text = $"Банк питань: {bank.Questions.Count} питань ({bankSource})";
        lblControlDesc.Text = lblControlDesc.Text.Replace("15 хв", $"{bank.Settings.ControlTimeMinutes} хв");
        lblAuthor.Text = $"Автор: {AppInfo.Author}, група {AppInfo.Group} · версія {AppInfo.Version}";

        // Режим викладача з'явиться на етапі 6
        lnkTeacher.Enabled = false;

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

        // Ховаємо стартову сторінку на час тесту і повертаємо її після закриття вікна тесту
        Hide();
        TestResult? result;

        using (TestForm test = new(session))
        {
            test.ShowDialog();
            result = test.Result;   // null — учень вийшов, не завершивши тест
        }

        if (result != null)
        {
            // Розбір помилок: у «Навчанні» завжди, у «Контрольній» — якщо дозволив викладач
            bool showReview = mode == TestMode.Training || bank.Settings.ShowReviewAfterControl;

            using ResultForm resultForm = new(result, showReview);
            resultForm.ShowDialog();
        }

        Show();
    }

    // ===================== Посилання внизу =====================

    private void LnkAbout_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using AboutForm about = new();
        about.ShowDialog(this);
    }

    private void LnkTeacher_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        // Режим викладача буде додано на етапі 6
    }

    // ===================== Допоміжне =====================

    // Прибирає зайві пробіли: "  Іваненко   Олена " -> "Іваненко Олена"
    private static string NormalizeSpaces(string text)
    {
        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(this, message, "Перевірте дані", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}

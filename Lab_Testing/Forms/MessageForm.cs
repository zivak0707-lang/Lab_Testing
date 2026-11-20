namespace Lab_Testing;

// Вид повідомлення: від нього залежать значок і кнопки
public enum MessageKind
{
    Info,
    Warning,
    Error,
    Question
}

// Власне вікно повідомлення з українськими кнопками «Так» / «Ні» / «OK».
// Стандартний MessageBox підписує кнопки мовою Windows («Да/Нет», «Yes/No»), тому не підходить
public partial class MessageForm : Form
{
    public MessageForm(string text, string title, MessageKind kind, bool yesNo)
    {
        InitializeComponent();

        Text = title;
        lblText.Text = text;
        lblText.MaximumSize = new Size(LogicalToDeviceUnits(380), 0);
        picIcon.Image = SystemIcons.GetStockIcon(IconFor(kind), LogicalToDeviceUnits(36)).ToBitmap();

        // Кнопки: «Так» і «Ні» для запитання, «OK» для звичайного повідомлення
        btnYes.Visible = yesNo;
        btnNo.Visible = yesNo;
        btnOk.Visible = !yesNo;
        AcceptButton = yesNo ? btnYes : btnOk;
        CancelButton = yesNo ? btnNo : btnOk;

        // Висота вікна — під довжину тексту
        int textBottom = lblText.Top + lblText.PreferredSize.Height;
        int iconBottom = picIcon.Bottom;
        ClientSize = new Size(ClientSize.Width,
            Math.Max(textBottom, iconBottom) + LogicalToDeviceUnits(22) + pnlButtons.Height);
    }

    private static StockIconId IconFor(MessageKind kind)
    {
        return kind switch
        {
            MessageKind.Warning => StockIconId.Warning,
            MessageKind.Error => StockIconId.Error,
            MessageKind.Question => StockIconId.Help,
            _ => StockIconId.Info
        };
    }

    // Звільняємо картинку значка разом із вікном
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        picIcon.Image?.Dispose();
        base.OnFormClosed(e);
    }
}

// Короткі виклики повідомлень (замість MessageBox.Show)
public static class Dialogs
{
    public static void Info(IWin32Window? owner, string text, string title)
    {
        Show(owner, text, title, MessageKind.Info, yesNo: false);
    }

    public static void Warning(IWin32Window? owner, string text, string title)
    {
        Show(owner, text, title, MessageKind.Warning, yesNo: false);
    }

    public static void Error(IWin32Window? owner, string text, string title)
    {
        Show(owner, text, title, MessageKind.Error, yesNo: false);
    }

    // Запитання «Так / Ні»: true — натиснули «Так»
    public static bool Ask(IWin32Window? owner, string text, string title, bool warning = false)
    {
        MessageKind kind = warning ? MessageKind.Warning : MessageKind.Question;
        return Show(owner, text, title, kind, yesNo: true) == DialogResult.Yes;
    }

    private static DialogResult Show(IWin32Window? owner, string text, string title, MessageKind kind, bool yesNo)
    {
        using MessageForm form = new(text, title, kind, yesNo);

        if (owner == null)
        {
            form.StartPosition = FormStartPosition.CenterScreen;
            return form.ShowDialog();
        }

        // Власник «поверх усіх» (контрольна на весь екран): повідомлення теж має бути поверх усіх.
        // Інакше Windows відкриє його ПІД вікном тесту, і тест чекатиме на відповідь у невидимому вікні
        if (WindowsApi.IsTopMost(owner.Handle))
        {
            form.TopMost = true;
        }

        return form.ShowDialog(owner);
    }
}

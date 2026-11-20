namespace Lab_Testing;

// Скільки копій програми відкрито і чи триває зараз контрольна.
// Навіщо: поки в одній копії йде контрольна, в іншій можна було б відкрити «Навчання»
// і перевіряти відповіді кнопкою «Перевірити». Тому:
//  - контрольна не починається, якщо відкрита ще одна копія програми;
//  - під час контрольної нова копія не відкривається, а контрольна записує цю спробу.
// Використовуються іменовані об'єкти Windows (семафор, м'ютекс, подія) — звичайний спосіб,
// яким програми дізнаються одна про одну. Назва exe-файлу не важлива: перейменована копія теж помітна
public static class InstanceGuard
{
    private const string InstancesName = @"Local\Lab_Testing.Instances";
    private const string ControlName = @"Local\Lab_Testing.ControlRunning";
    private const string SecondCopyName = @"Local\Lab_Testing.SecondCopy";

    // Лічильник копій: кожна копія займає одне «місце» семафора, поки працює
    private const int MaxCopies = 1000;
    private static Semaphore? instances;
    private static bool registered;

    // Програма запустилась: займаємо місце (викликається один раз у Program.Main)
    public static void Register()
    {
        instances = new Semaphore(MaxCopies, MaxCopies, InstancesName);
        registered = instances.WaitOne(0);
    }

    // Програма закривається: звільняємо місце
    public static void Unregister()
    {
        if (registered)
        {
            instances?.Release();
            registered = false;
        }
    }

    // Скільки інших копій програми зараз відкрито
    public static int OtherCopies
    {
        get
        {
            using Semaphore counter = new(MaxCopies, MaxCopies, InstancesName);

            if (!counter.WaitOne(0))
            {
                return MaxCopies;   // місць немає зовсім — отже, копій дуже багато
            }

            // Узяли одне місце на мить і повернули його. До повернення вільних було previous,
            // зайнятих — MaxCopies - previous, з них одне — наше тимчасове
            int previous = counter.Release();
            int copies = MaxCopies - previous - 1;
            return Math.Max(0, copies - (registered ? 1 : 0));
        }
    }

    // У якійсь копії програми зараз іде контрольна
    public static bool ControlIsRunning
    {
        get
        {
            if (Mutex.TryOpenExisting(ControlName, out Mutex? mutex))
            {
                mutex.Dispose();
                return true;
            }

            return false;
        }
    }

    // Нова копія повідомляє копію з контрольною, що її намагалися відкрити
    public static void NotifyControl()
    {
        if (EventWaitHandle.TryOpenExisting(SecondCopyName, out EventWaitHandle? signal))
        {
            signal.Set();
            signal.Dispose();
        }
    }

    // Позначка «іде контрольна» на час контрольної. onSecondCopy викликається (у фоновому потоці),
    // коли хтось намагається відкрити ще одну копію програми
    public static IDisposable BeginControl(Action onSecondCopy)
    {
        return new ControlScope(onSecondCopy);
    }

    private sealed class ControlScope : IDisposable
    {
        private readonly Mutex marker;
        private readonly EventWaitHandle signal;
        private readonly RegisteredWaitHandle wait;

        public ControlScope(Action onSecondCopy)
        {
            marker = new Mutex(false, ControlName);
            signal = new EventWaitHandle(false, EventResetMode.AutoReset, SecondCopyName);
            wait = ThreadPool.RegisterWaitForSingleObject(signal, (_, _) => onSecondCopy(), null, Timeout.Infinite,
                executeOnlyOnce: false);
        }

        public void Dispose()
        {
            wait.Unregister(null);
            signal.Dispose();
            marker.Dispose();
        }
    }
}

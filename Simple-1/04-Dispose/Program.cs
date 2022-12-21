/*
    Интерфейс IDisposabe используется для того,
    чтобы автоматически удалить некоторые объекты, которые необходимо удалять вручную

    Попробуем реализовать класс, который будет автоматически менять параметры консоли:
    например, делать текст красным. А затем будет всё возвращать назад
*/

Thread.GetDomain().ProcessExit += (o, e) => {};


Console.WriteLine("Обычный текст");

// Делаем преднамеренную ошибку: вместо using var используем просто var
// Далее мы полчим текст защиты, которая скажет, что мы забыли вызвать Dispose
var errOpts = new RedConsoleOptions();

Console.WriteLine("Красный текст");

using (var opts = new GreenConsoleOptions())
{
    Console.WriteLine("Зелёный текст");
}

// Вот так, кстати, вызывать не получится, ибо Dispose есть явная реализация IDisposable
// errOpts.Dispose();
// errOpts.Disposing(); - вот так можно
// Вот так тоже можно вызывать Dispose
// (errOpts as IDisposable).Dispose();

Console.WriteLine("Ошибочно красный текст");

// Для иллюстрации защиты, обнуляем ссылку на errOpts и вызываем сборщик мусора
errOpts = null;
GC.Collect();
GC.WaitForPendingFinalizers();

Console.WriteLine("Снова обычный текст, т.к. наша защита восстановила его");


// В этом классе мы сохраним первоначальный цвет консоли: текста и фона
public abstract class ConsoleOptions: IDisposable
{
    public ConsoleColor InitialBackgroundColor;
    public ConsoleColor InitialForegroundColor;

    // Мы будем использовать класс SafeHandle для того, чтобы просто гарантировать вызов деструктора в .NET
    // Непосредственно хранить и освобождать дескриптор нам не нужно
    // Мы его используем именно потому, что он хорошо позволяет нам гарантированно освобождать объект
    class SH : System.Runtime.InteropServices.SafeHandle
    {
        // Здесь будем хранить объект, который мы хотим обезопасить
        ConsoleOptions? obj;
        public SH(ConsoleOptions obj): base(-1, true)
        {
            this.obj    = obj;
            this.SetHandle(0);
        }

        public override bool IsInvalid => obj == null;

        protected override bool ReleaseHandle()
        {
            Console.WriteLine("ReleaseHandle for class " + this.GetType());
            obj?.Disposing(true);

            obj = null;
            this.handle = -1;

            return true;
        }
    }

    public ConsoleOptions()
    {
        InitialBackgroundColor = Console.BackgroundColor;
        InitialForegroundColor = Console.ForegroundColor;

        // GC.ReRegisterForFinalize(this); - этот метод тоже не поможет вызову в .NET
        // Регистрируем SafeHandle
        destructor = new SH(this);

        // Если этого не сделать, то
        // защита сработает в .NET Framework, но не сработает в .NET
        // т.к. Framework предпринимает усилия, чтобы вызвать деструкторы при завершении работы,
        // а .NET, наоборот, деструкторы не вызывает
    }

    // Здесь мы будем восстанавливать первоначальный цвет
    public virtual void Disposing(bool fromDestructor = false)
    {
        // Чтобы было понятно, когда кто вызывается, пишем отладочный вывод
        Console.WriteLine("Disposing for class " + this.GetType());

        // Если функция случайно вызвана повторно, то просто ничего не будем делать
        // Иногда, в таких случаях, стоит вызывать исключения.
        if (Disposed)
            return;

        Console.BackgroundColor = InitialBackgroundColor;
        Console.ForegroundColor = InitialForegroundColor;

        Disposed = true;
        // На этот объект мы больше не будем вызывать деструктор, т.к. уже всё очистили
        // Этот вызов не обязателен
        GC.SuppressFinalize(this);

        // Если сработала защита от забывания вызова IDisposable.Dispose()
        if (fromDestructor)
        {
            // В данном случае, просто сообщим на консоль об этом
            Console.Error.WriteLine("ERROR: IDisposable.Dispose() was not called");
        }
    }

    /*
        Это реализация функции Dispose соответствующего интерфейса
        Обратите внимание, мы в ней только лишь вызываем виртуальную функцию Disposing()
        И больше ничего не делаем.
        Почему?
        Dispose() не является виртуальной,
        и мы не сможем легко переопределить эту функцию в наследниках.
        А вот Disposing() - функция виртуальная.

        IDisposable.Dispose() будет вызвана автоматически, если используется с ключевым словом using
    */
    // public void Dispose() - альтернативное определение, которое позволяет вызывать Dispose() напрямую
    // В данном случае Dispose не будет доступен для вызова напрямую.
    // То есть obj.Dispose() вызвать не получится
    void IDisposable.Dispose()
    {
        Disposing();
    }

    protected bool Disposed = false;

    // Давайте сделаем защиту от забывания вызова IDisposable.Dispose()
    // Для этого определим деструктор
    // В нём вызовем Disposing с флагом, что это именно забытый Disposing
    // Хотя в .NET Framework эта защита работает, в .NET - нет.
    // Поэтому, для этого мы переопределим SafeHandle
    // Кроме этого, при аварийном завершении работы,
    // SafeHandle с большей вероятностью вызовутся, чем деструкторы даже в .NET Framework
    ~ConsoleOptions()
    {
        Console.WriteLine("~ConsoleOptions for class " + this.GetType());
        Disposing(true);
    }
    
    SH destructor;

    /*
    // Определение деструктора - тоже самое, как если бы мы сделали переопределение Finalize
    // Однако Finalize запрещает переопределять компилятор CS0249
    protected override void Finalize()
    {
        try
        {
            Disposing(true);
        }
        finally
        {
            base.Finalize();
        }
    }
    */
}

// Определяем класс, который делает цвет текста другим
public class RedConsoleOptions: ConsoleOptions
{
    // Для лаконичности конструктора, используем лямбды
    public RedConsoleOptions() => Console.ForegroundColor = ConsoleColor.Red;
}

public class GreenConsoleOptions: ConsoleOptions
{
    public GreenConsoleOptions() => Console.ForegroundColor = ConsoleColor.Green;
}

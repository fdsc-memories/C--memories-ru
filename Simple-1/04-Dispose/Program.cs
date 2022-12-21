/*
    Интерфейс IDisposabe используется для того,
    чтобы автоматически удалить некоторые объекты, которые необходимо удалять вручную

    Попробуем реализовать класс, который будет автоматически менять параметры консоли:
    например, делать текст красным. А затем будет всё возвращать назад
*/

// Это работающая защита, но неудобная
using var exitEvent = new ExitEventClass();

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

Console.WriteLine("Снова ошибочно красный текст, хотя всё должно было уже сработать");
// Даже после конца приложения, Dispose не вызовется


// Для того, чтобы это работало, нужно либо подписаться на Thread.GetDomain().ProcessExit, как выше, только с удалением объектов
// Либо сделать и вставить вызов своего события, которое вручную будет вызываться в конце программы
// ProcessExit ограничен на выполнение временем (стандартно - 2 секунды), так что лучше его не использовать
Thread.GetDomain().ProcessExit += (o, e) =>
{
    Console.WriteLine("Теперь строка должна быть нормального цвета");
};


// В этом классе мы сохраним первоначальный цвет консоли: текста и фона
public abstract class ConsoleOptions: IDisposabe_checkOnExit
{
    public ConsoleColor InitialBackgroundColor;
    public ConsoleColor InitialForegroundColor;

    // Вот это тоже не будет работать
    // Мы будем использовать класс SafeHandle для того, чтобы гарантировать вызов деструктора в .NET
    // Непосредственно хранить и освобождать дескриптор нам не нужно
    // Мы его используем именно потому, что он хорошо позволяет нам гарантированно освобождать объект
    // Однако, освобождать объект реально он тоже не будет
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
        // Вот это наша основная работающая защита. К сожалению, её надо вставлять вручную
        ExitEventClass.addObject(this);

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
    public virtual void Disposing(bool fromDestructor = true)
    {
        // Чтобы было понятно, когда кто вызывается, пишем отладочный вывод
        Console.WriteLine("Disposing for class " + this.GetType());

        // Если функция случайно вызвана повторно, то просто ничего не будем делать
        // Иногда, в таких случаях, стоит вызывать исключения.
        if (disposed)
            return;

        Console.BackgroundColor = InitialBackgroundColor;
        Console.ForegroundColor = InitialForegroundColor;

        disposed = true;
        // На этот объект мы больше не будем вызывать деструктор, т.к. уже всё очистили
        // Этот вызов не обязателен
        GC.SuppressFinalize(this);

        // Если сработала защита от забывания вызова IDisposable.Dispose()
        if (fromDestructor)
        {
            // В данном случае, просто сообщим на консоль об этом
            Console.Error.WriteLine("ERROR: IDisposable.Dispose() was not called");

            // Здесь мы вызовем исключение, чтобы
            throw new Exception();
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
        Disposing(false);
    }

    public bool disposed {get; protected set;} = false;

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


interface IDisposabe_checkOnExit: IDisposable
{
    public bool disposed {get;}
}

class ExitEventClass : IDisposable
{
    protected object sync = new Object();

    protected static ExitEventClass? ExitEventObject = null;
    public ExitEventClass()
    {
        lock (sync)
        {
            if (ExitEventObject != null)
                throw new InvalidOperationException();

            onExit += (o, a) => {};
            ExitEventObject = this;
        }
    }

    public bool disposed { get; private set; } = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // TODO: освободить управляемое состояние (управляемые объекты)
            }

            onExit(this, new EventArgs());

            Console.WriteLine("ExitEventClass.Dispose");

            // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
            // TODO: установить значение NULL для больших полей
            disposed = true;
        }
    }

    // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
    // ~ExitEventClass()
    // {
    //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    protected event EventHandler onExit;

    public static void addObject(IDisposabe_checkOnExit obj)
    {
        if (ExitEventObject == null)
            throw new ArgumentNullException("call 'using var exitEvent = new ExitEventClass();' before in the main function");

        var st = new System.Diagnostics.StackTrace(true);

        ExitEventObject.onExit += (o, e) =>
        {
            try
            {
                if (!obj.disposed)
                {
                    obj.Dispose();
                    Console.Error.WriteLine("ExitEventClass.onExit: error for object \n" + st);
                }
            }
            catch
            {}
        };
    }

    public void Dispose()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

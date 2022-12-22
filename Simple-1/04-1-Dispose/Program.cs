/*
    Простой пример использования IDisposabe в папке 04-0-Dispose

    Попробуем реализовать класс, который будет автоматически менять параметры консоли:
    например, делать текст красным. А затем будет всё возвращать назад

    При этом, будем проверять в конце работы программы, не забыли ли мы вернуть назад параметры консоли.

    Для того, чтобы IDisposable работал, нужно использовать ключевое слово using, или делать всё самостоятельно
*/

// Это работающая защита от забывания вернуть параметры консоли назад. Работающая, но неудобная
// Благодаря слову using, exitEvent.Dispose() будет вызван в конце работы данной функции (в конце работы программы)
using var exitEvent = new ExitEventClass();

// Если зарегистрирован обработчик UnhandledException, то необработанные исключения будут вызывать это событие
// В таком случае, исключение, произошедшее в блоке using будет отрабатывать верно:
// сначала будет вызван Dispose объекта, созданного с помощью using, а затем уже будет что-то ещё
// В противном случае, к сожалению, Dispose не вызывается (21 декабря 2022 года, Linux)
// То есть когда есть полностью неперехваченное исключение, Dipose не вызывается
/*
Thread.GetDomain().UnhandledException += (o, e) =>
{
    Console.WriteLine("Произошло исключение");

    exitEvent.Dispose();
};
*/

Console.WriteLine("Обычный текст");

// Делаем преднамеренную ошибку: вместо using var используем просто var
// Объект errOpts регистрируется в exitEvent непосредственно в конструкторе класса
// Поэтому, в конце работы программы получим текст защиты, которая скажет, что мы забыли вызвать Dispose
var errOpts = new RedConsoleOptions();

Console.WriteLine("Красный текст");

using (var opts = new GreenConsoleOptions())
{
    Console.WriteLine("Зелёный текст");
    // Если это исключение не обрабатываемое вообще нигде, то оно, почему-то, будет препятствовать вызову Dispose
    // throw new Exception();
}

try
{
    using var opts2 = new GreenConsoleOptions();
    Console.WriteLine("Зелёный текст 2");

    // Здесь исключение не страшно: всё равно будет вызван Dispose
    throw new Exception();
}
catch
{}


// Вот так, кстати, вызывать не получится, ибо метод Dispose есть явная реализация IDisposable (объявлен как IDisposable.Dispose)
// errOpts.Dispose();
// errOpts.Disposing(); - вот так можно
// Вот так тоже можно вызывать Dispose
// (errOpts as IDisposable).Dispose();

Console.WriteLine("Ошибочно красный текст (должен быть белый)");

// Для иллюстрации защиты, обнуляем ссылку на errOpts и вызываем сборщик мусора
errOpts = null;
GC.Collect();
GC.WaitForPendingFinalizers();  // Ждём все финализаторы

Console.WriteLine("Снова ошибочно красный текст, хотя всё должно было уже сработать");
// Даже после конца приложения, Dispose и деструктор не вызовется в .NET


// Для того, чтобы принудительное удаление в конце приложения работало, нужно либо подписаться на Thread.GetDomain().ProcessExit;
// Либо сделать и вставить вызов своего события, которое вручную будет вызываться в конце программы
// ProcessExit ограничен на выполнение временем (стандартно - 2 секунды), так что лучше его не использовать
// Поэтому здесь мы лишь вставляем надпись, чтобы проверить, что наша защита сработала
Thread.GetDomain().ProcessExit += (o, e) =>
{
    Console.WriteLine("Теперь строка должна быть нормального цвета");

    // На всякий случай добавляем сюда защиту, хотя она должна вызываться при конце программы (ПЕРЕД ProcessExit)
    exitEvent.Dispose();
};


void stackOverflow()
{
    stackOverflow();
};

// Если у нас будет исключение, защита штатно сработает, только если это исключение где-то поймано
// throw new Exception();


// Иллюстрация. Если у нас будет переполнение стека,
// то защита всё равно не сработает - вообще никогда; StackOverflow - неперехватываемое исключение
// stackOverflow();





// В этом классе мы сохраним первоначальный цвет консоли: текста и фона
// И восстановим его после использования
/*
Штатное использование получится
using (var opts = new ConsoleOptions())
{
    выводим нечто в консоль;
}

После using консоль возвращается к прежней работе благодаря вызову IDisposabe.Dispose()

Мы реализуем не IDisposabe, а IDisposabe_checkOnExit, но от этого смысл не меняется.
(IDisposabe_checkOnExit определён ниже как наследник IDisposabe)
*/
public abstract class ConsoleOptions: IDisposabe_checkOnExit
{
    public ConsoleColor InitialBackgroundColor;
    public ConsoleColor InitialForegroundColor;

    // Вот это тоже не будет работать
    // Мы будем использовать класс SafeHandle для того, чтобы гарантировать вызов деструктора в .NET
    // Непосредственно хранить и освобождать дескриптор (handle) нам не нужно
    // Однако, освобождать объект реально он тоже не будет, то есть не пригоден для той защиты, которую мы хотим
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
        // Вот это наша основная работающая защита. К сожалению, её надо вставлять вручную в каждый конструктор
        ExitEventClass.addObject(this);

        InitialBackgroundColor = Console.BackgroundColor;
        InitialForegroundColor = Console.ForegroundColor;

        // GC.ReRegisterForFinalize(this); - этот метод тоже не поможет вызову в .NET
        // Регистрируем SafeHandle
        destructor = new SH(this);

        // защита сработает в .NET Framework, но не сработает в .NET
        // т.к. .NET Framework предпринимает усилия, чтобы вызвать деструкторы при завершении работы,
        // а .NET, наоборот, деструкторы не вызывает никогда
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
    EventHandler? IDisposabe_checkOnExit.onExitHandler {get; set;} = null;

    // Давайте сделаем защиту от забывания вызова IDisposable.Dispose()
    // Для этого определим деструктор
    // В нём вызовем Disposing с флагом, что это именно забытый Disposing
    // Хотя в .NET Framework эта защита работает, в .NET - нет.
    ~ConsoleOptions()
    {
        Console.WriteLine("~ConsoleOptions for class " + this.GetType());
        Disposing(true);
    }
    
    SH destructor;

    /*
    // Определение деструктора - тоже самое, как если бы мы сделали переопределение Finalize
    // Однако Finalize запрещает переопределять компилятор - ошибка CS0249
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

// Определяем класс, который делает цвет текста другим - для красного цвета
public class RedConsoleOptions: ConsoleOptions
{
    // Для лаконичности конструктора, используем лямбды
    public RedConsoleOptions() => Console.ForegroundColor = ConsoleColor.Red;
}

// Определяем класс, который делает цвет текста другим - для зелёного цвета
public class GreenConsoleOptions: ConsoleOptions
{
    public GreenConsoleOptions() => Console.ForegroundColor = ConsoleColor.Green;
}

// Определяем интерфейс, который будет дополнять IDisposable
// Его задача - показать нам, что объект нормально удалён или нет
interface IDisposabe_checkOnExit: IDisposable
{
    public bool          disposed      {get;}
    public EventHandler? onExitHandler {get; set;}
}

// Вот этот класс будет реализовывать защиту от забывания использовать using
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
        if (disposed)
            return;

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

    // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
    // ~ExitEventClass()
    // {
    //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    protected event EventHandler onExit;

    // Эта функция позволяет нам зарегистрировать объект
    public static void addObject(IDisposabe_checkOnExit obj)
    {
        if (ExitEventObject == null)
            throw new ArgumentNullException("call 'using var exitEvent = new ExitEventClass();' before in the main function");

        var st = new System.Diagnostics.StackTrace(true);

        obj.onExitHandler = (o, e) =>
        {
            try
            {
                if (!obj.disposed)
                {
                    obj.Dispose();
                    Console.Error.WriteLine("ExitEventClass.onExit: the forget object error for the object by:\n" + st);

                    releaseObject(obj);
                }
            }
            catch
            {}
        };

        ExitEventObject.onExit += obj.onExitHandler;
    }

    // Мы должны разрегистрировать объект, чтобы позволить сборщику мусора его удалить
    // Иначе объект будет висеть до бесконечности
    public static void releaseObject(IDisposabe_checkOnExit obj)
    {
        if (ExitEventObject == null)
            throw new ArgumentNullException();

        ExitEventObject.onExit -= obj.onExitHandler;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

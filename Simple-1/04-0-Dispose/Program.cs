/*
    Интерфейс IDisposabe используется для того,
    чтобы автоматически удалить некоторые объекты, которые необходимо удалять вручную
*/

// sc в конце функци (в данном случае - в конце программы) компилятор автоматически вызовет sc.Dispose
using var sc = new SimpleClass();


using (var ssc = new SimpleSealedClass())
{

}
// После блока using, ssc автоматически вызовет Dispose

// Забытый using - это не удалится
// в .NET не поможет даже вызов Dispose в деструкторе (они там не вызываются; только в .NET Framework)
var scForget = new SimpleClass();

// В .NET не помогает даже многократный вызов сборщика мусора: деструкторы всё равно не вызываются
for (int i = 0; i < 100; i++)
{
    var a = new byte[1024*1024];
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

// Чтобы помочь scForget удалится, можно лишь вручную вызвать этот метод
// scForget.Dispose();


// Класс без наследования. Просто реализуем интерфейс
sealed class SimpleSealedClass: IDisposable
{
    // Этот метод явно реализует IDisposable
    // Вызвать obj.Dispose() напрямую не получится
    // Можно только (obj as IDisposable).Dispose()
    void IDisposable.Dispose()
    {
        Console.WriteLine("SimpleSealedClass.Dispose");

        // Говорим сборщику мусора, что у этого объекта уже не нужно вызывать деструктор
        // Это делать не обязательно
        GC.SuppressFinalize(this);
    }
}

// Этот класс может иметь наследников
// Это плохо, т.к. IDisposable.Dispose() сам по себе не является виртуальной функцией 
// и не может быть переопределён в наследниках; а это может понадобиться
class SimpleClass: IDisposable
{
    // Чтобы процедуры уничтожения можно было переопределить в потомках,
    // нам нужно перенести эти процедуры в виртуальную функцию
    public void Dispose()
    {
        // Здесь мы только вызовем нужную виртуальную функцию
        Dispose(fromDestructor: false);
        GC.SuppressFinalize(this);
    }

    // Если будет повторный вызов, нам не нужно заново удалять объекты
    // Поэтому мы делаем в объекте флаг, говорящий нам о том, что объект уже был "удалён"
    public bool disposed {get; private set;} = false;

    // Если disposing == false, значит мы забыли вызвать Dispose
    // Обратим внимание, этот метод - виртуальный. Его уже можно переопределять в потомках
    protected virtual void Dispose(bool fromDestructor = true)
    {
        if (disposed)
            return;

        Console.WriteLine("SimpleClass.Dispose");

        if (fromDestructor)
            Console.WriteLine("SimpleClass.Dispose: произошла ошибка: забыт вызов Dispose");
    }

    // Если хотим, вставляем проверку в деструктор
    // Однако в современной .NET это бессмысленно,
    // т.к. деструкторы, похоже, вообще никогда не вызываются
    ~SimpleClass()
    {
        Console.WriteLine("~SimpleClass()");
        Dispose();
    }
}

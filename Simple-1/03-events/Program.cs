namespace _03_events;
class Program
{
    // Событие на основе EventHandler
    // 1. Переопределяем свои аргументы события, создавая класс-наследник от EventArgs (если надо)
    public class CustomEventArgs : EventArgs
    {
        public CustomEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }

    // 2. Объявляем делегат
    // Объявление делегата похоже на объявление функции
    public delegate void CustomEventHandler(object sender, CustomEventArgs args);

    // 3. Объявляем непосредственно само событие
    // Варианты: стандартный обработчик; свой обработчик; параметризованный стандартный разработчик
    public class EventsGeneratorClass
    {
        // Это аналогичное событие со стандартным делегатом
        // public event EventHandler? CustomEvent1 = null;
        public event CustomEventHandler? CustomEvent2 = null;       // Это наше событие
        public event EventHandler<CustomEventArgs> CustomEvent3;    // Это параметризованное стандартное событие

        public EventsGeneratorClass()
        {
            // Делаем фиктивную подписку на событие, чтобы оно никогда не было null
            // Тогда нам не придётся проверять его на null так, как мы это делаем ниже с CustomEvent2
            // Здесь мы подписываем пустую лямбду на событие
            // Отписаться от такой подписки невозможно, т.к. ссылку на лямбду мы здесь не запоминаем
            CustomEvent3 += (sender, CustomEvent3) => {};
        }

        public void RaiseEvent2()
        {
            // Вызовем одно из событий
            // Создаём копию события. Для чего это нужно?
            // Если мы просто проверим CustomEvent2 на null,
            // а другой поток сразу после этого отпишется от события
            // он, таким образом, обнулит CustomEvent2
            // В таком случае мы можем работать с CustomEvent2 тогда, когда он неожиданно станет null,
            // хотя CustomEvent2 и прошёл проверку на то, что не равен null, но всё равно null
            // Копия в переменной никогда не обнулится, если мы её уже проверили
            // Такой приём описан здесь:
            // https://learn.microsoft.com/ru-ru/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines

            // Такое копирование мы можем сделать только из этого класса
            var CustomEvent2_copy = this.CustomEvent2;

            if (CustomEvent2_copy != null)
            {
                CustomEvent2_copy(new object(), new CustomEventArgs("Event 2"));
            }

            // Это более краткий пример вызова, полностью аналогичный вышеприведённому
            this.CustomEvent2?.Invoke(new object(), new CustomEventArgs("Event 2 from Invoke"));
        }

        public void RaiseEvent3()
        {
            // Здесь вызываем напрямую, т.к. один обработчик у нас всегда подписан: это пустой EmptyHandler
            // Следовательно, CustomEvent3 никогда не null
            CustomEvent3(new object(), new CustomEventArgs("Event 3"));
        }
    }

    // 4. Класс, который хочет обрабатывать события
    class EmptyHandlers
    {
        public EmptyHandlers(EventsGeneratorClass egc)
        {
            egc.CustomEvent2 += CustomEvent3_Handler;
            egc.CustomEvent3 += CustomEvent3_Handler;
        }

        // Боевой обработчик
        private void CustomEvent3_Handler(object? sender, CustomEventArgs args)
        {
            Console.WriteLine("CustomEvent3_Handler: " + args.Message);
        }

        public void RemoveHandlers()
        {
            egc.CustomEvent2 -= CustomEvent3_Handler;
            egc.CustomEvent3 -= CustomEvent3_Handler;
        }
    }

    static EventsGeneratorClass egc = new EventsGeneratorClass();
    static void Main(string[] args)
    {
        var a = new EmptyHandlers(egc);
        egc.RaiseEvent2();
        egc.RaiseEvent3();

        // Отписываемся от событий: теперь обработчик не будет вызываться
        a.RemoveHandlers();
        egc.RaiseEvent2();
        egc.RaiseEvent3();

        a.ToString();
        /*
            Программа выдаёт следующее:
                CustomEvent3_Handler: Event 2
                CustomEvent3_Handler: Event 2 from Invoke
                CustomEvent3_Handler: Event 3

            Программа вызывает обработчик EmptyHandlers.CustomEvent3_Handler дважды из функций RaiseEvent2 и RaiseEvent3
        */
    }
}

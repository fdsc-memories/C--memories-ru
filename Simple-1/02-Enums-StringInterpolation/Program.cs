/*
    @Типы перечислений: Enum (@enum) @Перечисления
    @Интерполяция строк

    Запуск программы:
    dotnet run
    или
    dotnet build
    dotnet bin/Debug/net7.0/Enums.dll
*/

// Декларация имени пространства имён: распространяется на весь файл
// Пространство имён может быть произвольным
using System.Text;

namespace Enums;

// Начинаем с базового класса, который содержит точку входа в программу: функцию Main
class Program
{
    // Main может иметь разные прототипы
    // static int Main(string[] args)
    static void Main(string[] args)
    {
        // Строки, начинающиеся с $ являются интерполируемыми
        // (содержат подстановочные шаблоны в фигурных скобках)
        // Обратим внимание на то, что кавычки в фигурных скобках не нужно экранировать обратными слешами
        Console.WriteLine($"Enum1.Denied={Enum1.Denied}");
        Console.WriteLine($"G: {Enum1.Denied.ToString("G")}, F: {Enum1.Denied.ToString("F")}, D: {Enum1.Denied.ToString("D")}, X: {Enum1.Denied.ToString("X")}");
        Console.WriteLine($"Enum1.Denied={Enum1_String[(int) Enum1.Denied]}");
        Console.WriteLine($"Enum1.Denied={Enum1_Dict[Enum1.Denied.ToString()]}");
        // Вывод:
        // Enum1.Denied=Denied
        // G: Denied, F: Denied, D: 2, X: 00000002
        // Enum1.Denied=Запрещено
        // Enum1.Denied=Запрещено
        
        // Перечисление допустимых имён типа Enum
        string[] names = Enum.GetNames(typeof(Enum1));
        Console.WriteLine("Члены перечисления {0}:", typeof(Enum1).Name);

        foreach (var name in names)
        {
            var status = (Enum1) Enum.Parse(typeof(Enum1), name);
            // Console.WriteLine("   {0} {1} = {0:D} [{Enum1_Dict[Enum1.Denied.ToString()]}]", name, status);
            // \t \n и т.п. - стандартные подстановочные символы: табуляция, новая строка
            // \u4f60 - так можно кодировать символы Unicode
            // Подстановки {0} используются без доллара. {0} - первый аргумент (в данном случае, name), {1} - второй аргумент функции (status)
            Console.WriteLine("   \"{0}\"\t= {1}\t= {1:D}\t= [{2}]", name, status, Enum1_Dict2[status]);
        }
        /* Вывод:
        Члены перечисления Enum1:
            "Granted"	= Granted	= 1	= [Разрешено]
            "Denied"	= Denied	= 2	= [Запрещено]
        */

        var en2 = Enum2.AB;

        // Проверяем, какие флаги установлены в переменной en2
        Console.WriteLine("en2 has A: " + en2.HasFlag(Enum2.A));
        Console.WriteLine("en2 has B: " + en2.HasFlag(Enum2.B));
        Console.WriteLine("en2 has C: " + en2.HasFlag(Enum2.C));

        /* Вывод:
            en2 has A: True
            en2 has B: True
            en2 has C: False
        */

        Console.WriteLine();
        Console.WriteLine("en2.ToString (): " + en2);
        Console.WriteLine("en2.ToString2(): " + en2.ToString2());

        /* Вывод:
            en2.ToString (): AB
            en2.ToString2(): [A, B, AB]

            Обратим внимание, что здесь есть все явно объявленные флаги: AB тоже.
            Но флага BC нет, т.к. он явно не определён
        */
    }

    enum Enum1 { Granted = 1, Denied = 2}

    // Инициализатор строки для подстановок
    static string[] Enum1_String = { "", "Разрешено", "Запрещено" };

    // Ещё один из вариантов подстановок: Enum1_Dict[Enum1.Denied.ToString()]
    static SortedDictionary<string, string> Enum1_Dict = new SortedDictionary<string, string>
    {
        {  "Denied",    "Запрещено"  },
        {  "Granted",   "Разрешено"  }
    };

    // Другой из вариантов подстановок: Enum1_Dict[Enum1.Denied]
    static SortedDictionary<Enum1, string> Enum1_Dict2 = new SortedDictionary<Enum1, string>
    {
        {  Enum1.Denied,    "Запрещено"  },
        {  Enum1.Granted,   "Разрешено"  }
    };

    // Flags - особый атрибут: теперь тип перечисления можно использовать в качестве битовых флагов
    // В типе Enum2 удобно было бы определить константу None = 0, если это требуется
    [Flags]
    public enum Enum2 { A = 1, B = 2, C = 4, AB = 3, AC = 5 }
}


// Enum2 является классом. Хотя он не может содержать членов класса явно, его можно расширять
// Расширять классы можно только в статических классах верхнего уровня (во вложенных классах нельзя)
static class Extensions
{
    public static string ToString2(this Program.Enum2 en)
    {
        var sb = new StringBuilder("[");

        string[] names = Enum.GetNames(typeof(Program.Enum2));
        foreach (var name in names)
        {
            var flag = (Program.Enum2) Enum.Parse(typeof(Program.Enum2), name);
            if (en.HasFlag(flag))
            {
                if (sb.Length > 1)
                    sb.Append(", ");

                sb.Append(name);
            }

            Console.WriteLine("Name: " + name);
            /* Вывод:
                Name: A
                Name: B
                Name: AB
                Name: C
                Name: AC
            */
        }

        sb.Append("]");
        return sb.ToString();
    }
}

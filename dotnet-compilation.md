[Общая справка по API](https://learn.microsoft.com/ru-ru/dotnet/api/)

@dotnet
[Конкретная справка по утилите dotnet](https://learn.microsoft.com/ru-ru/dotnet/core/tools/dotnet)

---
### Вывести список установленных пакетов .NET
Для SDK (пакеты для разработки)
Для конечных пользователей

dotnet --list-sdks
dotnet --list-runtimes


---
### Создать проект в текущей (пустой) папке:

dotnet new console  --framework net7.0 --use-program-main
dotnet new classlib --framework net7.0
dotnet new sln


Консольный проект
    --use-program-main говорит об использовании явной декларации класса Program и функции Main
Библиотека классов
Решение

---
### Добавление пакетов
dotnet add package Figgle

### Использование dotnet sln для работы с решениями
https://learn.microsoft.com/ru-ru/dotnet/core/tools/dotnet-sln

Перечислить все проекты
Добавить проекты
Удалить проекты

dotnet sln list
dotnet sln add ./tests/exe/
dotnet sln remove ./tests/exe/

; Это у меня не работает
; dotnet sln todo.sln add (ls -r **/*.csproj)
; Добавляет все проекты, соответствующие шаблону, в решение
dotnet sln add **/*.csproj
dotnet sln add **/*/*.csproj

### Установка проектов
Ссылки на справку
[Общие сведения о размещении](https://learn.microsoft.com/ru-ru/dotnet/core/deploying/deploy-with-cli
[Усечение размера среды .NET, встроенного в dll](https://learn.microsoft.com/ru-ru/dotnet/core/deploying/trimming/trim-self-contained
[Список платформ, для которых возможна компиляция](https://learn.microsoft.com/ru-ru/dotnet/core/rid-catalog
[Прекомпиляция вместо jit-компиляции](https://learn.microsoft.com/ru-ru/dotnet/core/deploying/ready-to-run
-p:PublishReadyToRun=true
Параметр командной строки генерирует сборку в одном файле вместе с указанными зависимостями
[/p:PublishSingleFile=true](https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md)

Пример генерации: конфигурация Release для платформы Linux и архитектуры x64, среда .NET включена в сборку
dotnet publish -c Release -r linux-x64 --self-contained true

Флаг, который добавляет опцию генерации исполняемого файла, подогнанного под конкретную архитектуру, на которой выполняется сборка
--use-current-runtime true




dotnet add ./builder/ reference ./libs/utils/

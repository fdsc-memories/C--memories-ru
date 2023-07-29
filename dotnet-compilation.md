[Общая справка по API](https://learn.microsoft.com/ru-ru/dotnet/api/)

@dotnet
[Конкретная справка по утилите dotnet](https://learn.microsoft.com/ru-ru/dotnet/core/tools/dotnet)

[Справка по xml-файлам ms-build (в т.ч. Project)](https://learn.microsoft.com/ru-ru/visualstudio/msbuild/msbuild-project-file-schema-reference)
[Справка по свойствам ms-build внутри проекта](https://learn.microsoft.com/ru-ru/dotnet/core/project-sdk/msbuild-props#implicitusings)
[Свойства компилятора](https://learn.microsoft.com/ru-ru/dotnet/csharp/language-reference/compiler-options/language)
[Справка по переменным msbuild](https://learn.microsoft.com/ru-ru/visualstudio/msbuild/common-msbuild-project-properties?view=vs-2022)
[Целевые объекты msbuild](https://learn.microsoft.com/ru-ru/visualstudio/msbuild/msbuild-targets?view=vs-2022)
[Выполнение внешних команд](https://learn.microsoft.com/ru-ru/visualstudio/msbuild/how-to-extend-the-visual-studio-build-process?view=vs-2022)

Параметр Nullable - смотреть в C-sharp-comments.md


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

Вывести список возможных вариантов на данной системе
dotnet new list

---
### Добавление пакетов
dotnet add package Figgle

### Использование dotnet sln для работы с решениями
https://learn.microsoft.com/ru-ru/dotnet/core/tools/dotnet-sln

Создать решение (*.sln)
Перечислить все проекты
Добавить проекты
Удалить проекты

dotnet new sln
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
[Общие сведения о размещении](https://learn.microsoft.com/ru-ru/dotnet/core/deploying/deploy-with-cli)
[Усечение размера среды .NET, встроенного в dll](https://learn.microsoft.com/ru-ru/dotnet/core/deploying/trimming/trim-self-contained)
[Список платформ, для которых возможна компиляция](https://learn.microsoft.com/ru-ru/dotnet/core/rid-catalog)
[Прекомпиляция вместо jit-компиляции](https://learn.microsoft.com/ru-ru/dotnet/core/deploying/ready-to-run)
-p:PublishReadyToRun=true
Параметр командной строки генерирует сборку в одном файле вместе с указанными зависимостями
[/p:PublishSingleFile=true](https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md)

Пример генерации: конфигурация Release для платформы Linux и архитектуры x64, среда .NET включена в сборку
dotnet publish -c Release -r linux-x64 --self-contained true

Флаг, который добавляет опцию генерации исполняемого файла, подогнанного под конкретную архитектуру, на которой выполняется сборка
--use-current-runtime true

#### Пример публикации в один файл без включения зависимостей
dotnet publish --output ./build.dev -c Release --self-contained false --use-current-runtime true /p:PublishSingleFile=true

Также может быть и дополнительная прекомпиляция
dotnet publish --output ./build.dev -c Release --self-contained false --use-current-runtime true /p:PublishSingleFile=true -p:PublishReadyToRun=true



[Добавление ссылок на другие проекты](https://learn.microsoft.com/ru-ru/dotnet/core/tools/dotnet-add-reference)
dotnet add ./builder/ reference ./libs/utils/
dotnet list ./builder/ reference
dotnet remove ./builder/ reference


#### Некоторые интересные настройки проекта

<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="../../../../VinKekFish/src/main/1 BytesBuilder/bytesbuilder.csproj" />
    <ProjectReference Include="../../../../VinKekFish/src/main/3 cryptoprime/cryptoprime.csproj" />
    <ProjectReference Include="../../../../VinKekFish/src/main/4 utils/4 utils.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <RootNamespace>VinKekFish_console</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <!-- Мы не позволяем здесь unsafe-код, он должен быть вынесен весь в библиотеку -->
    <AllowUnsafeBlocks>false</AllowUnsafeBlocks>
    <!-- Компилятор пытается создать сборки, которые не изменяются от билда к билду (если не изменился код) -->
    <Deterministic>true</Deterministic>
    <!-- Предупреждения считать ошибками -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

    <!-- Это предупреждения, сгенерированные по директиве #warning в исходных кодах -->
    <WarningsNotAsErrors>CS1030</WarningsNotAsErrors>
    <!-- Это предупреждения, сгенерированные из-за недостаточной xml-документации -->
    <NoWarn>CS1591</NoWarn>
    
    <!-- Этот проект не оптимизируется, т.к. в этом проекте функции должны быть безопасны от тайминг-атак: ничто не должно быть убрано -->
    <Optimize>false</Optimize>

  </PropertyGroup>

</Project>


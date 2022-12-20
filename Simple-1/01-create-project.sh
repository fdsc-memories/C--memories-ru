# Создаёт новый проект в подпапке 1-empty
# Папка будет очищена

rm -rf ./1-empty
dotnet new console  --framework net7.0 --use-program-main --output ./1-empty --name empty

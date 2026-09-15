# CrossApp — Наскрізний проєкт з крос-платформного програмування

## Структура Solution
```text
CrossApp/
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── Dto/
    │   ├── Domain/
    │   └── Storage/
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

## Предметна область: Замовлення (Orders)
* **Призначення:** Оформлення замовлень покупців, розрахунок сум замовлень з урахуванням вартості позицій та управління статусами обробки.
* **Сутності:**
  * `Customer` — покупець / клієнт (ім'я, email, телефон, адреса доставки);
  * `Product` — товар каталогу (найменування, ціна, одиниця виміру, статус наявності);
  * `Order` — замовлення (дата, номер, зв'язок із клієнтом, статус виконання, підсумкова вартість);
  * `OrderLine` — рядок замовлення (посилання на товар, кількість, фіксована ціна на момент замовлення).

## Домовленість про каталоги в Core (на весь семестр)
* `Core/Dto/` — record-типи формату даних.
* `Core/Domain/` — сутності з поведінкою та інваріантами.
* `Core/Storage/` — реалізації сховищ даних.

## Команди для збірки, запуску та публікації

### Збірка
```bash
dotnet build
```
### Збірка окремо бібліотеки Core
```bash
dotnet build src/Core/Core.csproj
```

### Запуск застосунку через dotnet run
```bash
dotnet run --project src/Cli -f net10.0

dotnet run --project src/Cli -f net8.0
```

### Публікація в різних режимах
```bash
Self-contained публікація:
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -f net10.0 -o publish/self

Framework-dependent публікація:
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -f net10.0 -o publish/fd

Single-file
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -f net10.0 -p:PublishSingleFile=true -o publish/single

Trimmed
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -f net10.0 -p:PublishTrimmed=true -o publish/trimmed
```
### Запуск безпосередньо з каталогів publish
```bash
.\publish\self\Cli.exe
.\publish\fd\Cli.exe
.\publish\single\Cli.exe
.\publish\trimmed\Cli.exe
```

###
```bash
```

###
```bash
```

### Запуск CLI
```bash
dotnet run --project src/Cli
```

### Публікація
1. **Self-contained:**
   ```bash
   dotnet publish src/Cli -c Release -r win-x64 --self-contained true -f net10.0 -o publish/self
   ```
2. **Framework-dependent:**
   ```bash
   dotnet publish src/Cli -c Release -r win-x64 --self-contained false -f net10.0 -o publish/fd
   ```
3. **Single-file (додаткове завдання 1):**
   ```bash
   dotnet publish src/Cli -c Release -r win-x64 --self-contained true -f net10.0 -p:PublishSingleFile=true -o publish/single
   ```
4. **Trimmed (додаткове завдання 2):**
   ```bash
   dotnet publish src/Cli -c Release -r win-x64 --self-contained true -f net10.0 -p:PublishTrimmed=true -o publish/trimmed
   ```

## Порівняння режимів публікації

| RID          | Режим                       | Розмір publish               | Потрібен встановлений runtime? | Кількість файлів |
|--------------|-----------------------------|------------------------------|--------------------------------|------------------|
| win-x64      | self-contained              | ~76.68 МБ                    | ні                             | 194              |
| win-x64      | framework-dependent         | ~0.19 МБ                     | так (.NET 10)                  | 7                |
| win-x64      | self-contained (SingleFile) | ~70.15 МБ                    | ні                             | 3                |
| win-x64      | self-contained (Trimmed)    | ~19.16 МБ                    | ні                             | 31               |

### Різниця між self-contained та framework-dependent:
* **Self-contained публікація** містить код застосунку, залежності та вбудовану копію .NET Runtime для конкретної платформи, тому не потребує встановленого .NET на цільовій машині, але має більший розмір.
* **Framework-dependent публікація** містить лише зкомпільований код та залежності без середовища виконання, тому забезпечує мінімальний розмір, але вимагає наявності сумісного .NET Runtime на комп'ютері користувача.

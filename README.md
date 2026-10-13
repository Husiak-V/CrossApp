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

## Інваріанти доменної моделі (Замовлення)
* **Непорожні ідентифікатори:** ідентифікатор замовлення та клієнта не можуть бути порожніми (`ArgumentException`).
* **Коректність числових значень у рядку:** ціна товару не може бути від'ємною ($\ge 0$), а кількість має бути більшою за нуль ($> 0$) (`ArgumentOutOfRangeException`).
* **Заборона модифікації не-чернетки:** рядки можна додавати лише в статусі `Draft`; у `Confirmed`/`Cancelled` — `InvalidOperationException`.
* **Заборона підтвердження порожнього замовлення:** не можна підтвердити замовлення, яке не містить жодного рядка (`InvalidOperationException`).
* **Допустимі переходи статусів (дод. 3):** `Draft -> Confirmed`, `Draft -> Cancelled`, `Confirmed -> Cancelled`; решта переходів заборонена (`InvalidOperationException`, перевірка через `switch expression`).
* **Ліміт відкритих замовлень клієнта (дод. 2):** не більше 5 відкритих (`Draft`/`Confirmed`) замовлень на клієнта (`InvalidOperationException` у `OrderPolicy`; правило рівня сервісу, бо потребує доступу до сховища).
* **Імпорт із перевіркою інваріантів (дод. 1):** `OrderImport.ToEntities` бере `ImportResult<OrderLineDto>` і повертає сутності + помилки рядків, що не пройшли `FromDto`.

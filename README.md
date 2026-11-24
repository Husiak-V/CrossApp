# CrossApp — Наскрізний проєкт з крос-платформного програмування

## Структура Solution
```text
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
├── data/                          # sample.csv, sample.json, mixed.csv, order-lines*.json, orders-sample.json (lab09)
├── logs/                          # lab06: app.log, sinks.log (створюються під час запуску)
├── tests/
│   └── Core.Tests/                # lab08: xUnit, посилається лише на Core; lab09: Lab9Tests.cs
│       ├── Core.Tests.csproj
│       ├── TestData.cs
│       ├── TestImporters.cs
│       ├── OrderServiceTests.cs
│       ├── ReportServiceTests.cs
│       ├── ImportTests.cs
│       └── Lab9Tests.cs           # lab09: шляхи, JSON, export/import
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── DomainException.cs     # lab08: доменні бізнес-помилки
    │   ├── EnvironmentInfo.cs
    │   ├── SampleData.cs          # 20 демонстраційних замовлень
    │   ├── Abstractions/
    │   │   ├── IOrderStore.cs     # контракт сховища; lab09: ReplaceAll
    │   │   └── IOrderImporter.cs  # lab08: async-контракт імпорту
    │   ├── Configuration/         # lab09
    │   │   └── AppOptions.cs      # Storage, Orders, Logging, EnvironmentName
    │   ├── Domain/
    │   │   ├── Order.cs, OrderLine.cs, OrderStatus.cs
    │   │   └── OrderImport.cs, OrderPolicy.cs
    │   ├── Dto/
    │   ├── Events/                # lab06: доменні події
    │   │   ├── OrderChangeKind.cs
    │   │   └── OrderChangedEventArgs.cs
    │   ├── Import/
    │   │   ├── JsonOrderImporter.cs      # lab08: LoadAsync + LoadStreamAsync
    │   │   └── ImportTimeoutException.cs # lab08 (дод. 2)
    │   ├── Notifications/         # lab06 (дод. 1): порт INotificationSink
    │   │   └── INotificationSink.cs
    │   ├── Reports/               # lab07: DTO звітів і LINQ-запити
    │   │   ├── OrderReportLine.cs
    │   │   ├── GroupSummary.cs
    │   │   └── ReportService.cs
    │   ├── Services/
    │   │   └── OrderService.cs    # бізнес-операції, event Changed, ImportAsync, ExportJson/ImportJson
    │   └── Storage/
    │       ├── InMemoryOrderStore.cs
    │       ├── FileOrderStore.cs  # lab09: атомарний JSON і ReplaceAll
    │       ├── CachingOrderStore.cs   # декоратор (дод. 1)
    │       └── OrderJson.cs       # lab09: camelCase, кирилиця, .tmp + File.Move
    └── Cli/
        ├── Cli.csproj             # lab09: config-пакети, UserSecretsId, копіювання appsettings
        ├── appsettings.json       # lab09: базові ключі (без secrets)
        ├── appsettings.Development.json
        ├── appsettings.Production.json
        ├── Program.cs             # lab07: report-*; lab08: import-*; lab09: стек конфігурації
        ├── DataPaths.cs           # data/orders.json, data/*.csv, logs/app.log
        ├── DeferredExecutionDemo.cs # lab07: демо відкладеного виконання
        ├── ImportCommand.cs       # lab08: Ctrl+C, тайм-аут, обробка винятків
        ├── Lab9Command.cs         # lab09: export/import/orders/config
        ├── Lab6Scenario.cs        # lab06: сценарій подій (підкоманда lab6)
        ├── Notifications/         # lab06 (дод. 1): ConsoleSink, FileSink, CompositeSink
        └── StoreFactory.cs        # фабрика (дод. 3)
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

## Лабораторна 5: сервісний шар (інтерфейс, два сховища, ручний DI)

### Абстракція та сервіс
* **Інтерфейс:** `Core.Abstractions.IOrderStore` — `List()`, `GetById()` (`Order?`), `Add()`, `Update()`, `Remove()`.
* **Сервіс:** `Core.Services.OrderService` — залежить лише від `IOrderStore`, який передається через конструктор.
* **Операції:** `CreateOrder`, `AddLine`, `Confirm`, `Cancel`, `Remove`, `All`, `Find(string)`, `Find(Func<Order, bool>)` (дод. 2).
* `CreateOrder` застосовує `OrderPolicy` (не більше 5 відкритих замовлень на клієнта).

### Реалізації сховища
| Клас | Механіка |
|---|---|
| `InMemoryOrderStore` | `Dictionary` у пам'яті, початкові дані через конструктор (`SampleData.Orders()`), зникають після виходу |
| `FileOrderStore` | кеш у пам'яті + `EnsureLoaded` при першому зверненні + `Flush` у файл після кожної зміни |
| `CachingOrderStore` (дод. 1) | декоратор: приймає інший `IOrderStore`, додає кешування, контракт не змінюється |

На диск записується `List<OrderDto>` (JSON), а мапінг виконують `Order.ToDto` / `Order.FromDto`: формат файлу не диктує форму домену. Шлях визначає `DataPaths.OrdersFile()`: від `AppContext.BaseDirectory` знаходиться корінь репозиторію (наявність `CrossApp.slnx`), тому файл завжди лежить у `data/orders.json` кореня проєкту незалежно від конфігурації збірки.

### Схема залежностей
```text
Cli (Program.cs) → OrderService → IOrderStore → InMemoryOrderStore
                                               FileOrderStore
                                               CachingOrderStore(inner)
```

### Composition root (Program.cs)
```csharp
IOrderStore store = useFactory
    ? StoreFactory.Create(args)
    : useFile
        ? new FileOrderStore(dataPath)
        : new InMemoryOrderStore(SampleData.Orders());

if (useCache) store = new CachingOrderStore(store);
var service = new OrderService(store);
```

Конкретні класи створюються лише в точці входу; `OrderService` не містить слів `File` чи `Console`.

### Запуск режимів
Прапорці вибору сховища працюють з будь-якою підкомандою; нижче для прикладу використано `report-top`.
```bash
dotnet run --project src/Cli -f net10.0 -- report-top                  # InMemoryOrderStore, дані зникають після виходу
dotnet run --project src/Cli -f net10.0 -- --file report-top           # FileOrderStore, накопичення в data/orders.json
dotnet run --project src/Cli -f net10.0 -- --file --cache report-top   # FileOrderStore у декораторі CachingOrderStore
dotnet run --project src/Cli -f net10.0 -- --file --factory report-top # вибір сховища через StoreFactory
```

## Інваріанти доменної моделі (Замовлення)
* **Непорожні ідентифікатори:** ідентифікатор замовлення та клієнта не можуть бути порожніми (`ArgumentException`).
* **Коректність числових значень у рядку:** ціна товару не може бути від'ємною ($\ge 0$), а кількість має бути більшою за нуль ($> 0$) (`ArgumentOutOfRangeException`).
* **Заборона модифікації не-чернетки:** рядки можна додавати лише в статусі `Draft`; у `Confirmed`/`Cancelled` — `DomainException` (lab08).
* **Заборона підтвердження порожнього замовлення:** не можна підтвердити замовлення, яке не містить жодного рядка (`DomainException`).
* **Допустимі переходи статусів (дод. 3):** `Draft -> Confirmed`, `Draft -> Cancelled`, `Confirmed -> Cancelled`; решта переходів заборонена (`DomainException`, перевірка через `switch expression`).
* **Ліміт відкритих замовлень клієнта (дод. 2):** не більше 5 відкритих (`Draft`/`Confirmed`) замовлень на клієнта (`DomainException` у `OrderPolicy`; правило рівня сервісу, бо потребує доступу до сховища).
* **Імпорт із перевіркою інваріантів (дод. 1):** `OrderImport.ToEntities` бере `ImportResult<OrderLineDto>` і повертає сутності + помилки рядків, що не пройшли `FromDto`.

## Лабораторна 6: доменні події, делегати Action/Func, лямбди

### Що змінилося
* **`Core.Events.OrderChangeKind`** — види успішних змін: `OrderCreated`, `LineAdded`, `OrderConfirmed`, `OrderCancelled`, `OrderRemoved`.
* **`Core.Events.OrderChangedEventArgs`** — незмінні дані події: `OrderId`, `Kind`, `LinesCount`, `Total`, `At` (момент створення аргументів). Клас успадковує `EventArgs`, тому підходить для стандартного `EventHandler<T>`.
* **`OrderService.Changed`** — `public event EventHandler<OrderChangedEventArgs>? Changed;`. Це саме `event`, а не публічне поле-делегат: зовнішній код може лише підписатися (`+=`) або відписатися (`-=`), а викликати обробники може тільки сервіс.
* **`OrderService.OnChanged`** — єдина точка підняття події. Викликається **після** успішної зміни стану та збереження у сховище, тому виняток інваріанта не створює хибного повідомлення.
* **`OrderService.FindAll(Func<Order, bool>)`** — критерій пошуку задає викликач; тут потрібен результат, тому використовується `Func`, а не подія. Старий `Find(Func<Order, bool>)` залишено як сумісний псевдонім.
* **`Cli`** — підписка локальним методом (консоль + `logs/app.log`), друга підписка лямбдою із замиканням над лічильником, відписка через `-=`, сценарій відмови та демонстрація `FindAll`.
* **`Core` не містить `Console.WriteLine` і не знає про `logs/app.log`.** Форматування, консоль і файл — відповідальність `Cli`.

### Чому саме ці зміни є подіями
`OrderCreated`, `LineAdded`, `OrderConfirmed`, `OrderCancelled` і `OrderRemoved` — це завершені бізнес-факти, які вже відбулися й можуть цікавити інші компоненти (журнал, API, UI). Вони не є частиною розрахунку чи перевірки інваріантів, тому домен не повинен знати, хто їх обробляє. Невдала операція (порожнє замовлення, кількість `0`, неіснуючий `id`, перевищення ліміту) не є фактом зміни стану — подія не піднімається.

### Фрагмент оголошення та підняття
```csharp
public event EventHandler<OrderChangedEventArgs>? Changed;

private void OnChanged(Order order, OrderChangeKind kind)
{
    OrderChangedEventArgs args = new(order.Id, kind, order.Lines.Count, order.Total);
    Changed?.Invoke(this, args);          // підписники, якщо вони є
    _notificationSink?.Notify(args);      // дод. 1: альтернативний шлях через DI
}
```

Приклад операції:
```csharp
public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
{
    Order order = Require(orderId);
    order.AddLine(productId, name, price, quantity); // може кинути виняток
    _store.Update(order);
    OnChanged(order, OrderChangeKind.LineAdded);      // лише після успіху
}
```

### Підписники в Cli
```csharp
void OnOrderChanged(object? sender, OrderChangedEventArgs e)
{
    string line = OrderChangeText.Format(e);
    Console.WriteLine($"[event] {line}");
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnOrderChanged;   // перший підписник: консоль + файл

int changes = 0;
service.Changed += (_, e) => { changes++; /* замикання */ };

service.Changed -= OnOrderChanged;  // другий підписник залишається
```

`Path` до журналу: `Path.Combine(<корінь репозиторію>, "logs", "app.log")`; каталог створюється через `Directory.CreateDirectory` до першого запису. Зараз підписники — консоль і файл. На тижні 10 ними стане логер проєкту `Api`, на тижні 12 — компонент Blazor, який оновлюватиме таблицю; `Core` при цьому не змінюється.

### Запуск
```bash
dotnet run --project src/Cli -f net10.0 -- lab6
dotnet run --project src/Cli -f net10.0 -- lab6 --file
dotnet run --project src/Cli -f net10.0 -- lab6 --file --cache
```

### Очікуваний вивід (фрагмент)
```text
=== Лабораторна 6: доменні події, Action/Func, лямбди ===
Сховище: InMemoryOrderStore
Журнал подій: ...\logs\app.log

--- 1. Підписка: локальний метод + лямбда-лічильник ---
[event] 16:37:23 OrderCreated   ORD-92F86A5E → позицій: 0, сума: 0,00 грн
[event] 16:37:23 LineAdded      ORD-92F86A5E → позицій: 1, сума: 300,00 грн
[event] 16:37:23 LineAdded      ORD-92F86A5E → позицій: 2, сума: 550,00 грн
[event] 16:37:23 OrderConfirmed ORD-92F86A5E → позицій: 2, сума: 550,00 грн
Подій отримано лямбдою: 4
Послідовність: OrderCreated -> LineAdded -> LineAdded -> OrderConfirmed
Рядків у app.log: 4

--- 2. Відписка локального методу через -= ---
Скасовано: Замовлення ORD-92F86A5E ... Статус: Cancelled
Лічильник лямбди: 4 -> 5 (+1, лямбда ще підписана)
app.log: 4 -> 4 рядків (без нових)
```

`logs/app.log` містить рівно чотири рядки до відписки:
```text
16:37:23 OrderCreated   ORD-92F86A5E → позицій: 0, сума: 0,00 грн
16:37:23 LineAdded      ORD-92F86A5E → позицій: 1, сума: 300,00 грн
16:37:23 LineAdded      ORD-92F86A5E → позицій: 2, сума: 550,00 грн
16:37:23 OrderConfirmed ORD-92F86A5E → позицій: 2, сума: 550,00 грн
```

Після `-=` операція `Cancel` змінює дані, лямбда-лічильник збільшується, але нових рядків у консолі від локального методу й у `app.log` немає. Сценарій відмови показує, що виняток є, а лічильник і журнал не змінюються.

### Додаткове завдання 1: `INotificationSink`
* `Core.Notifications.INotificationSink` — порт `void Notify(OrderChangedEventArgs change)`.
* `Cli.Notifications.ConsoleSink`, `FileSink`, `CompositeSink` — реалізації та композиція.
* `Cli` передає сінк у конструктор: `new OrderService(new InMemoryOrderStore(), new CompositeSink(new ConsoleSink(), new FileSink(sinkPath, reset: false)))`.
* Демонстрація друкує три рядки `[sink] ...` і пише їх у `logs/sinks.log`, не використовуючи подію.

Порівняння з подією:

| Критерій | Подія `Changed` | `INotificationSink` через конструктор |
|---|---|---|
| Кількість одержувачів | довільна, визначається в Cli | один сінк (або композиція) |
| Зв'язок із Core | Core не знає одержувачів | Core знає інтерфейс порту |
| Додати нового клієнта | лише `+=` у Cli | нова реалізація + wiring у Cli |
| Тестування | підписка в тесті, перевірка аргументів | підміна fake-сінка |

Подія краще підходить для «повідомити всіх, кому цікаво»; сінк — коли сервісу потрібно гарантовано передати зміну конкретному одержувачу або коли одержувач є обов'язковою залежністю.

Порівняно з прямим викликом логера з `Core` подія дає два конкретні наслідки. По-перше, `Core` не залежить від консолі, файлової системи чи майбутнього API/Blazor: новий одержувач додається підпискою в `Cli` (або іншому клієнті) без зміни сервісу й домену. По-друге, тест може підписатися на `Changed`, отримати `OrderChangedEventArgs` і перевірити факт та дані зміни без перехоплення консольного виводу чи читання файлу.

### Додаткове завдання 2: `Action<string>` у методі імпорту
`ProductCsvImporter.Load(path, Action<string>? onSkipped = null)` викликає `onSkipped` для кожного рядка, який не вдалося розібрати. `MixedCsvImporter.Load` і `OrderImport.ToEntities` / `DraftFromImport` мають такий самий необов'язковий параметр. У `Cli` передається лямбда, що додає повідомлення у список і друкує його:

```csharp
ImportResult<ProductDto> imported = ProductCsvImporter.Load(csvPath, message =>
{
    skipped.Add(message);
    Console.WriteLine($"  пропущено: {message}");
});
```

Для `data/sample.csv` імпортується 11 товарів, а 4 некоректні рядки потрапляють і в `Errors`, і в `onSkipped`. `Action<string>` обрано тому, що метод має лише повідомити про пропуск і нічого не повертає; `Func` тут був би зайвим.

### Самоперевірка
1. `src/Core` не містить `Console` і не згадує `logs/app.log` — журналюванням займається лише `Cli`.
2. Кожна успішна змінююча операція піднімає рівно одну подію (у демо — 4 події для 4 операцій).
3. Операція, що впала на інваріанті, не піднімає подію.
4. Після `-=` локальний обробник мовчить, а лямбда-лічильник продовжує рахувати.
5. Подія оголошена як `event`, а не як публічне поле делегата.
6. `logs/app.log` формується через `Path.Combine`, каталог створюється кодом.

## Лабораторна 7: звіти через LINQ (Where, Select, GroupBy, топ-N, агрегати)

Домен — **Замовлення**, тому варіант із методички адаптовано так: топ-5 клієнтів за сумою, замовлення дорожче N, групування за статусом, середній чек і максимальне замовлення. Звіти живуть у `Core.Reports`, не знають про `Console` і повертають DTO, а не `Order`. `Program.cs` містить лише вибір підкоманди, виклик `ReportService` і форматування виводу.

### Запити

| Метод | Питання, на яке відповідає | Використані оператори |
|---|---|---|
| `TopOrders(source, n)` | Які замовлення мають найбільшу суму? | `OrderByDescending`, `ThenBy`, `Take`, `Select` |
| `AboveTotal(source, min)` | Які замовлення дорожчі за N? | `Where`, `OrderBy`, `ThenBy`, `Select` |
| `ByStatus(source)` | Як замовлення розподілені за статусами? | `GroupBy`, `Count`, `Sum`, `Select`, `OrderByDescending` |
| `TopCustomers(source, n)` | Які клієнти мають найбільшу суму? | `GroupBy`, `Count`, `Sum`, `OrderByDescending`, `ThenBy`, `Take` |
| `Totals(source)` | Який середній чек, максимум і підсумок? | `ToList`, `Count`, `Sum`, `Average`, `Max` |

Усі звіти приймають `IEnumerable<Order>` і повертають `IReadOnlyList<DTO>`; виняток — агрегатний `Totals`, який повертає іменований кортеж із чотирьох чисел. `TopOrders` і `TopCustomers` мають другий ключ сортування (`OrderId` / `CustomerId`), тому топ-N не залежить від порядку джерела.

### GroupBy з проєкцією

```csharp
source
    .GroupBy(o => o.Status)
    .Select(g => new GroupSummary(
        g.Key.ToString(),      // ключ групи — статус
        g.Count(),             // кількість замовлень у групі
        g.Sum(o => o.Total)))  // сума замовлень групи
    .OrderByDescending(s => s.Total)
    .ThenBy(s => s.Key, StringComparer.Ordinal)
    .ToList();
```

`GroupSummary` — record `(Key, Count, Total)`, `OrderReportLine` — record `(OrderId, CustomerId, Status, Total)`. Повертати `Order` не можна: контракт звіту не повинен залежати від доменної моделі, а `Order` містить рядки, інваріанти й поведінку, які клієнту звіту не потрібні.

### Підкоманди

```bash
dotnet run --project src/Cli -f net10.0 -- report-top
dotnet run --project src/Cli -f net10.0 -- report-top 3
dotnet run --project src/Cli -f net10.0 -- report-over 3000
dotnet run --project src/Cli -f net10.0 -- report-groups
dotnet run --project src/Cli -f net10.0 -- report-clients
dotnet run --project src/Cli -f net10.0 -- report-sum
dotnet run --project src/Cli -f net10.0 -- demo-deferred
```

Прапорці сховища (`--file`, `--cache`, `--factory`) можна додавати до будь-якої підкоманди, наприклад:

```bash
dotnet run --project src/Cli -f net10.0 -- --file --cache report-groups
```

### Вивід підкоманд

```text
> dotnet run --project src/Cli -f net10.0 -- report-top
TOP-5 ORDERS BY TOTAL
ID         CUSTOMER     STATUS             TOTAL
ORD-1011   CUST-006     Confirmed        7500,00
ORD-1005   CUST-003     Draft            5100,00
ORD-1013   CUST-001     Confirmed        5000,00
ORD-1017   CUST-005     Confirmed        4800,00
ORD-1015   CUST-003     Confirmed        4500,00
```

```text
> dotnet run --project src/Cli -f net10.0 -- report-over 3000
ORDERS OVER 3000,00
ID         CUSTOMER     STATUS             TOTAL
ORD-1007   CUST-004     Confirmed        3100,00
ORD-1009   CUST-005     Draft            3200,00
ORD-1001   CUST-001     Confirmed        3250,00
ORD-1015   CUST-003     Confirmed        4500,00
ORD-1017   CUST-005     Confirmed        4800,00
ORD-1013   CUST-001     Confirmed        5000,00
ORD-1005   CUST-003     Draft            5100,00
ORD-1011   CUST-006     Confirmed        7500,00
```

```text
> dotnet run --project src/Cli -f net10.0 -- report-groups
ORDERS BY STATUS
STATUS        COUNT        TOTAL
Confirmed         9     33360,00
Draft             8     19300,00
Cancelled         3      2850,00
```

```text
> dotnet run --project src/Cli -f net10.0 -- report-clients
TOP-5 CLIENTS BY TOTAL
CUSTOMER      COUNT        TOTAL
CUST-003          3     12300,00
CUST-006          3     10500,00
CUST-001          3      9250,00
CUST-005          3      8800,00
CUST-004          3      7300,00
```

```text
> dotnet run --project src/Cli -f net10.0 -- report-sum
TOTALS
ORDERS        TOTAL      AVERAGE          MAX
    20     55510,00      2775,50      7500,00
```

```text
> dotnet run --project src/Cli -f net10.0 -- demo-deferred
COUNT(before)     16
COUNT(after)      17
SNAPSHOT          17
SOURCE(cleared)    0
```

### Відкладене виконання

`demo-deferred` бере локальний список, отриманий із `service.All()` через `ToList()`, бо `All()` кожного разу повертає новий знімок. Рядок `source.Where(o => o.Total > minTotal)` лише будує запит: `Where` не перебирає джерело. Перший `Count()` запускає перебирання і повертає **16**. Після додавання `ORD-DEMO-01` той самий об’єкт запиту при повторному `Count()` бачить новий елемент і повертає **17**. Далі `ToList()` один раз матеріалізує результат у `SNAPSHOT` (17 елементів), тому після `source.Clear()` у знімку залишається 17, а в джерелі — 0. `snapshot.Count` читає властивість списку, а `query.Count()` — метод, який щоразу виконує запит.

### Порожнє джерело і стабільність

`report-top`, `report-over`, `report-groups` і `report-clients` на порожньому джерелі повертають порожній список і друкують лише заголовок. `report-sum` повертає `0` замість винятку `Sequence contains no elements`, бо `Totals` перевіряє `list.Count == 0` до виклику `Average`/`Max`. Перевірено запуском з `--file`, коли `data/orders.json` містить `[]`.

Два послідовні запуски `report-top` дають однаковий порядок рядків: основний ключ — `Total`, другий — `OrderId` через `ThenBy(..., StringComparer.Ordinal)`. Без другого ключа порядок замовлень з однаковою сумою залежав би від порядку джерела.

### Самоперевірка

1. `Program.cs` не містить `Where`/`GroupBy`; усі запити — в `ReportService`.
2. Два запуски `report-top` дають однаковий порядок завдяки `ThenBy(o => o.Id)`.
3. Порожній набір не спричиняє `InvalidOperationException`.
4. Кожен рядковий звіт повертає DTO, а не `Order`.
5. Використовується лише LINQ to Objects; `AsParallel` і `AsQueryable` відсутні.
6. `SampleData` містить 20 замовлень і 3 статуси, тому `GroupBy` показує три групи.

### Захист

1. `ReportService.ByStatus`: `g.Key` — значення, за яким згруповано (`OrderStatus`), `g.Count()` — кількість елементів групи, `g.Sum(o => o.Total)` — сума замовлень групи.
2. `report-groups` і `report-sum` показують групування та агрегати; `report-top` двічі — стабільний топ-N.
3. `demo-deferred` показує `16 -> 17` через повторне виконання відкладеного `Where` і `17` у матеріалізованому знімку.
4. `OrderReportLine` і `GroupSummary` — DTO: звіт не залежить від зміни `Order`, не передає назовні інваріанти та рядки замовлення.

## Лабораторна 8: async/await, скасування, доменні винятки, unit-тести Core

Домен — **Замовлення**, тому варіант методички адаптовано: асинхронний імпорт **рядків замовлення**, `DomainException` для правила «замовлення вже підтверджено», тест на суму після додавання рядків. Додатково реалізовано завдання 1 (потоковий `IAsyncEnumerable`) і 2 (зв'язані токени з тайм-аутом).

### Що стало асинхронним і чому

| Операція | Стан | Причина |
|---|---|---|
| `JsonOrderImporter.LoadAsync` | async | читання файлу — реальний I/O; `FileStream(useAsync: true)` + `JsonSerializer.DeserializeAsync`, без `Task.Run` |
| `JsonOrderImporter.LoadStreamAsync` | async iterator | додаткове завдання 1: `IAsyncEnumerable<OrderLine>` + `await foreach` |
| `OrderService.ImportAsync` / `ImportStreamAsync` | async | очікує імпортер, передає `ct`, піднімає `LineAdded` на кожен доданий рядок |
| `InMemoryOrderStore`, `ReportService` | sync | обчислення в пам'яті/ LINQ, без зовнішнього I/O |
| `FileOrderStore` | sync | лишається синхронним у межах lab08; перехід на async — на етапі EF Core/API (тижні 10–11) |

`await` не створює нового потоку: під час очікування файлового I/O потік вільний для іншої роботи. `Task.Run(() => File.ReadAllText(...))` тут не використовується, бо це лише перенесло б блокування в пул потоків.

### Ключові фрагменти

`LoadAsync` — справжній async-ввід, `ct` у кожному `await` і в циклі (DoD 2, 3; захист 2):
```csharp
ct.ThrowIfCancellationRequested();
await using var stream = new FileStream(
    path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
List<OrderLineDto> dtos =
    await JsonSerializer.DeserializeAsync<List<OrderLineDto>>(stream, Options, ct) ?? throw ...;
for (int i = 0; i < dtos.Count; i++)
{
    ct.ThrowIfCancellationRequested();
    result.Add(ToLine(dtos[i], i));   // ArgumentException -> DomainException
}
```

Потоковий імпорт (додаткове завдання 1):
```csharp
public async IAsyncEnumerable<OrderLine> LoadStreamAsync(
    string path, [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (OrderLineDto? dto in JsonSerializer
        .DeserializeAsyncEnumerable<OrderLineDto>(stream, Options, ct)
        .WithCancellation(ct))
    {
        ct.ThrowIfCancellationRequested();
        yield return ToLine(dto, number++);
    }
}
```
`[EnumeratorCancellation]` дозволяє `await foreach (... .WithCancellation(ct))` передати токен саме в ітератор. `ImportStreamAsync` додає рядок одразу після `yield`, тому скасування посеред потоку зберігає вже додані рядки.

Зв'язані токени (додаткове завдання 2):
```csharp
using var timeoutCts = new CancellationTokenSource(timeout);
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutCts.Token);
try { return await operation(linkedCts.Token); }
catch (OperationCanceledException ex)
    when (timeoutCts.IsCancellationRequested && !userToken.IsCancellationRequested)
{
    throw new ImportTimeoutException(timeout, ex);
}
```
Якщо скасувався `timeoutCts`, назовні йде `ImportTimeoutException`; якщо `userToken` — звичайний `OperationCanceledException`. Це дозволяє Cli показати різні повідомлення.

### DomainException vs ArgumentException

* `OrderLine.Create` з `quantity <= 0` → `ArgumentOutOfRangeException`: це некоректний аргумент виклику.
* `Order.AddLine` у `Confirmed` → `DomainException("... уже підтверджено ...")`: це порушення бізнес-правила, а не форми аргументу.
* `Order.Confirm` порожнього замовлення та заборонений перехід статусу → `DomainException`.
* `OrderPolicy.EnsureCanCreateOrder` при перевищенні ліміту → `DomainException`.
* `JsonOrderImporter` загортає `ArgumentException`/`ArgumentOutOfRangeException` у `DomainException($"Запис {i+1}: ...", ex)`, зберігаючи `InnerException`.
* `IOException`, `JsonException`, `FileNotFoundException` не маскуються під доменні: Cli обробляє їх окремими `catch`.

### Тести

```bash
dotnet test
dotnet test tests/Core.Tests/Core.Tests.csproj -f net10.0
```
Проєкт `tests/Core.Tests` посилається лише на `src/Core` і не залежить від `Cli`. У тестах використовується `InMemoryOrderStore`; диск задіяний лише в тесті зі зламаним записом, тимчасовий файл видаляється у `finally`.

| Тест | Що перевіряє | Очікування |
|---|---|---|
| `AddLine_WithNegativeQuantity_Throws_AndKeepsState` | інваріант кількості + незмінний стан | `ArgumentOutOfRangeException`, сума та рядки не змінилися |
| `AddLine_RaisesChangedExactlyOnce` | подія тижня 6 | рівно одна `LineAdded` з новою сумою |
| `AddLine_WhenOrderConfirmed_ThrowsDomainException` | бізнес-правило | `DomainException`, текст «уже підтверджено» |
| `AboveTotal_CountsOnlyOrdersAboveThreshold` (`[Theory]`) | звіт на 4 наборах | 4 / 2 / 1 / 0 замовлень |
| `TopOrders_IsDeterministic` | детермінований топ-N | `ORD-A`, `ORD-C`, `ORD-B` завдяки `ThenBy(Id)` |
| `Totals_OnEmptySource_ReturnsZeros` | порожнє джерело | нулі без винятку |
| `ImportAsync_WhenAlreadyCancelled_ThrowsOperationCanceled` | заздалегідь скасований токен | `OperationCanceledException` |
| `ImportAsync_AddsLines_AndRecalculatesTotal` | варіант «Замовлення» | 2 рядки, сума 40, дві події |
| `ImportAsync_WithBrokenRecord_ThrowsDomainException` | погані дані | `DomainException` з «Запис 2», стан не змінений |
| `ImportAsync_WhenOrderConfirmed_ThrowsDomainException` | імпорт у підтверджене замовлення | `DomainException` |
| `ImportStreamAsync_AddsLinesFromStream` | дод. 1 | 2 рядки, сума 40 |
| `LoadStreamAsync_ReadsJsonIncrementally` | дод. 1: реальний JSON + `WithCancellation` | `P-1`, `P-2`, `P-3` |
| `ImportStreamAsync_WhenCancelledMidStream_StopsAfterDeliveredLine` | дод. 1 + скасування | `OperationCanceledException`, у замовленні 1 рядок |
| `ImportAsync_WhenTimeoutElapses_ThrowsImportTimeout_NotUserCancellation` | дод. 2 | `ImportTimeoutException`, `userCts` не скасований |
| `ImportAsync_WhenUserCancels_ThrowsOperationCanceled_NotTimeout` | дод. 2 | `OperationCanceledException`, не `ImportTimeoutException` |

Поточний результат після лабораторної 9: **Passed: 25, Failed: 0** (18 тестів lab08 + 7 тестів lab09).

### Запуск і перевірка скасування

```bash
dotnet run --project src/Cli -f net10.0 -- import-lines ORD-1002
dotnet run --project src/Cli -f net10.0 -- import-stream ORD-1002
dotnet run --project src/Cli -f net10.0 -- import-lines ORD-1002 data/order-lines-bad.json
dotnet run --project src/Cli -f net10.0 -- import-lines ORD-1002 --timeout 1 --slow
dotnet run --project src/Cli -f net10.0 -- import-stream ORD-1002 --timeout 1 --slow
```
`--slow` — лише демонстраційний декоратор у Cli (затримка 300 мс на рядок), щоб тайм-аут спрацював на маленькому файлі. Ctrl+C під час імпорту також скасовує операцію через `Console.CancelKeyPress` і `CancellationTokenSource`.

Очікуваний вивід:
```text
> ... import-lines ORD-1002
Імпортовано рядків: 6

> ... import-lines ORD-1002 data/order-lines-bad.json
Дані відхилено: Запис 2: Кількість у рядку має бути більшою за нуль ...

> ... import-lines ORD-1002 --timeout 1 --slow
Імпорт перервано тайм-аутом 1 с.

> Ctrl+C
Імпорт скасовано.
```
Порядок `catch` у `Cli/ImportCommand.cs`: `ImportTimeoutException` → `OperationCanceledException` → `DomainException` → `FileNotFoundException`/`JsonException`, тому скасування не виглядає як збій, а доменна помилка не маскує технічну.

### Самоперевірка та DoD

1. `tests/Core.Tests` є в solution, посилається лише на `Core`.
2. Імпорт читає файл через `FileStream(useAsync: true)` і `DeserializeAsync`; `Task.Run` немає.
3. `CancellationToken` є в сигнатурах, перевіряється в циклах і передається в кожен `await`.
4. Cli реагує на Ctrl+C і тайм-аут повідомленням без стектрейсу.
5. Бізнес-помилки — `DomainException`, аргументи — `ArgumentException`, I/O та JSON — окремі технічні винятки.
6. 25 тестів зелені (18 з lab08 + 7 з lab09); є інваріанти, подія, звіт, скасування, імпорт, тайм-аут.
7. У розв'язку немає `async void` (крім дозволеного обробника події Ctrl+C), `.Result`, `.Wait()`.
8. README містить команди тестів і сценарії скасування; зміни закомічено як lab08.

## Лабораторна 9: шляхи, JSON, конфігурація, Secrets

Домен — **Замовлення**. Окремий вертикальний зріз не додавався: `export`/`import` працюють із наявною колекцією `Order`, а ключ варіанта — `Orders:Currency` (валюта в CLI-виводі). Контракт `IOrderStore` розширено методом `ReplaceAll`, щоб імпорт замінював колекцію цілком; це реалізовано в `InMemoryOrderStore`, `FileOrderStore` і `CachingOrderStore`.

### Що зроблено

| Вимога | Реалізація |
|---|---|
| Прибрати літерали шляхів | `StorageOptions.ResolveFullPath` + `Path.Combine`/`Path.GetFullPath`; у `src` немає `C:\`, `/home/` і ручного `"\\"` |
| appsettings | `src/Cli/appsettings.json`, `.Development.json`, `.Production.json` копіюються в output/publish |
| AppOptions | `Core.Configuration.AppOptions` + `config.Get<AppOptions>()` |
| CROSSAPP_ та CROSSAPP_DATA_PATH | `AddEnvironmentVariables(prefix: "CROSSAPP_")` + окреме читання `CROSSAPP_DATA_PATH` |
| JSON export/import | `OrderService.ExportJson`/`ImportJson`, `OrderJson`, `FileOrderStore.ReplaceAll` |
| Secret | `config["Integration:ApiKey"]`; друкується лише «присутній, довжина N» |
| Переносимість | базовий каталог — `AppContext.BaseDirectory`, тому cwd не впливає |
| README | профілі, таблиця ключів, PowerShell/bash, user-secrets |

### Ключі конфігурації

| Ключ | appsettings.json | Призначення |
|---|---|---|
| `EnvironmentName` | `Development` | ім'я профілю в діагностиці |
| `Logging:MinimumLevel` | `Information` | рівень логування (Development — `Debug`, Production — `Warning`) |
| `Storage:DataPath` | `data` | каталог даних відносно `AppContext.BaseDirectory` |
| `Storage:FileName` | `orders.json` | файл колекції замовлень |
| `Orders:Currency` | `грн` | валюта в командах `orders` і `config` |
| `Integration:ApiKey` | **немає** | secret: user-secrets або `CROSSAPP_Integration__ApiKey` |

### Порядок джерел

```text
appsettings.json
  → appsettings.{CROSSAPP_ENVIRONMENT}.json
  → user-secrets
  → змінні середовища CROSSAPP_
  → аргументи командного рядка (--Ключ=Значення)
```

Пізніше джерело перекриває попереднє. `CROSSAPP_DATA_PATH` обробляється окремо після binder і перекриває саме `Storage:DataPath`.

### Профілі

| Профіль | Logging:MinimumLevel | Storage:DataPath |
|---|---|---|
| Development | `Debug` | `data` → `src/Cli/bin/Debug/net10.0/data/orders.json` |
| Production | `Warning` | `data/prod` → `.../data/prod/orders.json` |

Перемикання: `CROSSAPP_ENVIRONMENT=Production`.

### Команди

```bash
dotnet run --project src/Cli -f net10.0 -- export
dotnet run --project src/Cli -f net10.0 -- import
dotnet run --project src/Cli -f net10.0 -- orders
dotnet run --project src/Cli -f net10.0 -- config
```

`export` без аргументу пише у файл із конфігурації; якщо колекція порожня, спочатку додає 20 демонстраційних замовлень із `SampleData`. `import` без аргументу читає `data/orders-sample.json`. Явний шлях можна передати першим аргументом: `export data/orders-export.json`, `import data/orders-export.json`.

Команди лабораторної 9 працюють саме з `Storage:FileName`, а не з legacy-шляхом `--file` часів лабораторної 5. Прапорець `--file` і далі використовує `data/orders.json` кореня репозиторію для звітів та імпорту рядків.

Очікуваний вивід:

```text
> dotnet run --project src/Cli -f net10.0 -- export
Файл даних : C:\...\src\Cli\bin\Debug\net10.0\data\orders.json
ApiKey: присутній, довжина 13
Експортовано 20 замовлень.

> dotnet run --project src/Cli -f net10.0 -- import
Файл даних : C:\...\src\Cli\bin\Debug\net10.0\data\orders.json
ApiKey: присутній, довжина 13
Імпортовано 3 замовлень.

> dotnet run --project src/Cli -f net10.0 -- orders
ID         CUSTOMER     STATUS             TOTAL
ORD-9001   CUST-101     Draft            1700,00 грн
ORD-9002   CUST-102     Confirmed         500,00 грн
ORD-9003   CUST-103     Cancelled         400,00 грн
```

`data/orders-sample.json`:

```json
[
  {
    "id": "ORD-9001",
    "customerId": "CUST-101",
    "status": "Draft",
    "lines": [
      { "productId": "PRD-01", "name": "Цемент М400", "price": 150.0, "quantity": 10 }
    ]
  }
]
```

### Перевизначення шляху

PowerShell:

```powershell
$env:CROSSAPP_DATA_PATH = "C:\temp\crossapp-data"
$env:CROSSAPP_Storage__DataPath = "C:\temp\crossapp-data"
$env:CROSSAPP_Integration__ApiKey = "from-env"
$env:CROSSAPP_ENVIRONMENT = "Production"
dotnet run --project src/Cli -f net10.0 -- export
```

bash:

```bash
export CROSSAPP_DATA_PATH=/tmp/crossapp-data
export CROSSAPP_Storage__DataPath=/tmp/crossapp-data
export CROSSAPP_Integration__ApiKey=from-env
CROSSAPP_ENVIRONMENT=Production dotnet run --project src/Cli -f net10.0 -- export
```

Командний рядок (лише форма з `=`):

```bash
dotnet run --project src/Cli -f net10.0 -- export "--Storage:DataPath=C:\temp\crossapp-data"
```

`Storage:DataPath` у змінній середовища записується як `CROSSAPP_Storage__DataPath`: префікс `CROSSAPP_`, двокрапка → подвійне підкреслення.

### Secrets

```bash
dotnet user-secrets init --project src/Cli
dotnet user-secrets set "Integration:ApiKey" "<your-local-key>" --project src/Cli
dotnet user-secrets list --project src/Cli
```

`UserSecretsId` записано в `src/Cli/Cli.csproj` і комітиться; сам `secrets.json` лежить поза репозиторієм:

* Windows: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
* Linux/macOS: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

У консолі друкується лише факт наявності та довжина. `Integration:ApiKey` немає в `appsettings.json`; `git grep` за значенням секрету порожній. Для CI/продакшену використовується `CROSSAPP_Integration__ApiKey`, а не user-secrets.

### Чому Path.Combine, а не конкатенація

```csharp
string root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
Directory.CreateDirectory(root);
return Path.GetFullPath(Path.Combine(root, FileName));
```

`Path.Combine` ставить `\` у Windows і `/` у Linux/macOS, не дублює роздільники та коректно працює з абсолютним `DataPath`. `Path.GetFullPath` робить шлях абсолютним і нормалізує `..`. База — `AppContext.BaseDirectory` (каталог застосунку), а не `Environment.CurrentDirectory`, тому запуск з кореня solution і з `src/Cli` дає той самий файл:

```text
C:\...\src\Cli\bin\Debug\net10.0\data\orders.json
```

`Environment.CurrentDirectory` залежить від того, звідки запущено процес. `SpecialFolder.LocalApplicationData` доречний для користувацьких даних/кешу, які не повинні лежати поруч із бінарниками, а не для файлу колекції в цій роботі.

### JSON: camelCase, кирилиця, атомарний запис

```csharp
private static readonly JsonSerializerOptions Options = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
```

* `CamelCase` під час серіалізації дає `id`, `customerId`, `status`, `lines`, `productId`, `name`, `price`, `quantity`; під час десеріалізації — зіставляє їх із властивостями `Id`, `CustomerId`, ... `PropertyNameCaseInsensitive` додатково приймає PascalCase.
* `UnsafeRelaxedJsonEscaping` не перетворює `Ц` на `\u0426`. Для файлу даних це безпечно; для JSON, що вбудовується в HTML, потрібен звичайний encoder.
* Запис: `File.WriteAllText(path + ".tmp", ...)`, потім `File.Move(tmp, path, overwrite: true)`. Читач бачить або старий, або новий повний файл, а не напівзаписаний.
* Зіпсований JSON перетворюється на `InvalidDataException` з назвою файлу; `Cli` друкує «Файл пошкоджено: ...» без stack trace. `import` валідує всі записи до `ReplaceAll`, тому часткового імпорту немає.

### Тести лабораторної 9

| Тест | Що перевіряє |
|---|---|
| `StorageOptions_ResolveFullPath_IsAbsolute_AndCreatesDirectory` | абсолютний шлях від базового каталогу + створення `data/` |
| `FileOrderStore_Add_WritesReadableJsonAtomically` | camelCase, кирилиця без `\uXXXX`, відсутність `.tmp` |
| `FileOrderStore_LoadsExistingJson_AndChecksInvariants` | читання PascalCase і повторна перевірка інваріантів |
| `FileOrderStore_WithBrokenJson_ThrowsInvalidDataException` | зрозуміле повідомлення для зіпсованого JSON |
| `OrderService_ExportImport_RoundTrips` | повний цикл export → import зі статусами й рядками |
| `OrderService_Import_WithBrokenRecord_KeepsExistingCollection` | некоректний запис не замінює колекцію частково |
| `OrderService_Import_WithEmptyConfirmedOrder_ThrowsInvalidDataException` | доменний інваріант під час імпорту |

```bash
dotnet build CrossApp.slnx
dotnet test
```

Поточний результат: **Passed: 25, Failed: 0**.

### Самоперевірка та DoD

1. `export`/`import` працюють; `OrderService_ExportImport_RoundTrips` перевіряє повний цикл.
2. У `src` немає `C:\`, `/home/` і ручного `\`; шляхи будуються через `Path.Combine`.
3. `appsettings.json` є в репозиторії, без `Integration:ApiKey`.
4. `CROSSAPP_DATA_PATH` перекриває `Storage:DataPath` (перевірено експортом у тимчасовий каталог).
5. Secret читається з user-secrets або `CROSSAPP_Integration__ApiKey`; запуск з довільного cwd дає той самий файл.
6. `dotnet build` і `dotnet test` зелені (25 тестів); зміни закомічено як `lab09`.

### Захист: короткі відповіді

1. `CurrentDirectory` — звідки запущено процес, може бути будь-яким; `BaseDirectory` — каталог збірки/застосунку, стабільний для пошуку `appsettings.json` і даних.
2. `Path.Combine` враховує роздільник ОС і не створює подвійних роздільників; ручне склеювання з `"\"` ламається на Linux/macOS.
3. `LocalApplicationData` — для локальних даних користувача (кеш, налаштування), які не мають бути в каталозі застосунку або репозиторії.
4. `appsettings.json` → профіль → user-secrets → `CROSSAPP_` env → командний рядок; кожне наступне перекриває попереднє.
5. `CROSSAPP_Storage__DataPath` (префікс + `__` замість `:`).
6. Windows `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`, Linux/macOS `~/.microsoft/usersecrets/<id>/secrets.json`; user-secrets — для локальної розробки, у продакшені — env або сховище секретів.
7. `CamelCase` задає відображення імен під час серіалізації/десеріалізації; разом із `PropertyNameCaseInsensitive` читаються обидва варіанти регістру.
8. `System.Text.Json` за замовчуванням екранує не-ASCII; `UnsafeRelaxedJsonEscaping` лишає кирилицю читабельною.
9. `.tmp` + `File.Move` дає атомарну заміну: немає моменту, коли файл частково записаний.



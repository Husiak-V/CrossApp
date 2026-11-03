# CrossApp — Наскрізний проєкт з крос-платформного програмування

## Структура Solution
```text
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
├── data/                          # sample.csv, sample.json, mixed.csv (імпортери)
├── logs/                          # lab06: app.log, sinks.log (створюються під час запуску)
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── SampleData.cs          # 18 демонстраційних замовлень
    │   ├── Abstractions/
    │   │   └── IOrderStore.cs     # контракт сховища
    │   ├── Domain/
    │   │   ├── Order.cs, OrderLine.cs, OrderStatus.cs
    │   │   └── OrderImport.cs, OrderPolicy.cs
    │   ├── Dto/
    │   ├── Events/                # lab06: доменні події
    │   │   ├── OrderChangeKind.cs
    │   │   └── OrderChangedEventArgs.cs
    │   ├── Import/
    │   ├── Notifications/         # lab06 (дод. 1): порт INotificationSink
    │   │   └── INotificationSink.cs
    │   ├── Services/
    │   │   └── OrderService.cs    # бізнес-операції + event Changed
    │   └── Storage/
    │       ├── InMemoryOrderStore.cs
    │       ├── FileOrderStore.cs
    │       └── CachingOrderStore.cs   # декоратор (дод. 1)
    └── Cli/
        ├── Cli.csproj
        ├── Program.cs             # composition root + сценарій
        ├── DataPaths.cs           # data/orders.json, data/*.csv, logs/app.log
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
```bash
dotnet run --project src/Cli -f net10.0                  # InMemoryOrderStore, дані зникають після виходу
dotnet run --project src/Cli -f net10.0 -- --file        # FileOrderStore, накопичення в data/orders.json
dotnet run --project src/Cli -f net10.0 -- --file --cache  # FileOrderStore у декораторі CachingOrderStore
dotnet run --project src/Cli -f net10.0 -- --file --factory # вибір сховища через StoreFactory
```

## Інваріанти доменної моделі (Замовлення)
* **Непорожні ідентифікатори:** ідентифікатор замовлення та клієнта не можуть бути порожніми (`ArgumentException`).
* **Коректність числових значень у рядку:** ціна товару не може бути від'ємною ($\ge 0$), а кількість має бути більшою за нуль ($> 0$) (`ArgumentOutOfRangeException`).
* **Заборона модифікації не-чернетки:** рядки можна додавати лише в статусі `Draft`; у `Confirmed`/`Cancelled` — `InvalidOperationException`.
* **Заборона підтвердження порожнього замовлення:** не можна підтвердити замовлення, яке не містить жодного рядка (`InvalidOperationException`).
* **Допустимі переходи статусів (дод. 3):** `Draft -> Confirmed`, `Draft -> Cancelled`, `Confirmed -> Cancelled`; решта переходів заборонена (`InvalidOperationException`, перевірка через `switch expression`).
* **Ліміт відкритих замовлень клієнта (дод. 2):** не більше 5 відкритих (`Draft`/`Confirmed`) замовлень на клієнта (`InvalidOperationException` у `OrderPolicy`; правило рівня сервісу, бо потребує доступу до сховища).
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
dotnet run --project src/Cli -f net10.0
dotnet run --project src/Cli -f net10.0 -- --file
dotnet run --project src/Cli -f net10.0 -- --file --cache
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




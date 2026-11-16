# HSE_Software_Design_HW-2 — ПОС (Преподавательская Оценка Студентов)
### Коростелев Александр, студент БПИ-258

## Используемые технологии
**Stack**: C# on .NET 10 (SDK-style projects), console application\
**Architecture**: Clean Architecture (облегчённая), 4 проекта по слоям + проект тестов\
**Libraries**: Microsoft.Extensions.DependencyInjection (DI container), xUnit + Moq (unit testing)

**Tests coverage check**: coverlet.collector, отчет в формате Cobertura XML\
(**Result**: Line coverage **94.24%**, Branch coverage **84.04%**)

## Быстрый старт
Из корневой директории решения


**Запуск приложения (меню):**
```
dotnet build
dotnet run --project src/Pos.ConsoleApp
```

**Сквозной демо-сценарий без меню:**
```
dotnet run --project src/Pos.ConsoleApp -- --demo
```

**Запуск тестов**:
```
dotnet test
```

**Проверка покрытия тестами**:
```
dotnet test --collect:"XPlat Code Coverage"
```

## Демонстрация
Демо (`--demo` или пункт главного меню `Run the end-to-end demo`) проходит сквозной сценарий из ТЗ:

1. рубрикатор из трёх критериев (`Homework`, `Project`, `Exam`) собирается через `RubricBuilder`;
2. заводятся три студента;
3. выставляются оценки;
4. попытки нарушить правила отклоняются: 12 баллов из 10, несуществующий критерий, отрицательный балл, повторная оценка по тому же критерию;
5. одна оценка исправляется, другая отменяется;
6. итог считается по двум правилам (Strategy) и выводится таблицей;
7. итог экспортируется в CSV и JSON.

Из меню демо запускается только на пустых данных (иначе имена студентов и рубрикатор конфликтовали бы с уже введёнными), после него с данными можно продолжить работу через меню.

Меню: `Students`, `Criteria (rubric)`, `Evaluations`, `Final grades and export`. В любом поле ввода можно написать `cancel`, чтобы прервать текущее действие. Ошибки ввода и нарушения правил выводятся сообщением, приложение не падает.

## Структура и границы модулей

```
HSE_Software_Design_HW-2/
├── PosGrading.slnx
├── Directory.Build.props        # общие настройки: net10.0, nullable, implicit usings
├── src/
│   ├── Pos.Domain/              # Common, Students, Rubrics, Evaluations, Grading, Exceptions
│   ├── Pos.Application/         # Services, Facade, Dtos, Mapping, Abstractions, Errors
│   ├── Pos.Infrastructure/      # Persistence (in-memory репозитории), Export (CSV, JSON)
│   └── Pos.ConsoleApp/          # Program.cs, PosConsoleApp, Screens, Menu, Io, DemoScenario
└── tests/Pos.Tests/             # Domain, Application, Infrastructure, EndToEnd
```

| Модуль (проект) | Ответственность | Зависит от |
|---|---|---|
| `Pos.Domain` | Сущности, value objects, инварианты, `RubricBuilder`, domain service `FinalGradeCalculator`, правила итога, порты-репозитории | ни от чего |
| `Pos.Application` | Сценарии (`StudentService`, `RubricService`, `EvaluationService`, `ResultService`), фасад `IPosFacade`, DTO и маппинг, порт экспорта `IResultExporter` | `Domain` |
| `Pos.Infrastructure` | Адаптеры портов: in-memory репозитории, экспорт в CSV и JSON | `Application` (и через него `Domain`) |
| `Pos.ConsoleApp` | Консольный UI (меню, экраны, ввод/вывод, демо) и Composition Root в `Program.cs` | `Application`, `Infrastructure` |

**Стрелка зависимостей** (все зависимости направлены внутрь, к домену):

```
Pos.ConsoleApp ──► Pos.Application ──► Pos.Domain
Pos.Infrastructure ──► Pos.Application
Pos.ConsoleApp ──► Pos.Infrastructure   (только для регистрации адаптеров в DI)
```

Направление зависимостей закреплено ссылками между проектами: например, `using Pos.Infrastructure` внутри `Pos.Domain` или `Pos.Application` просто не скомпилируется. Порты объявлены во внутренних слоях (`IStudentRepository`, `IRubricRepository`, `IEvaluationRepository` в `Domain`, `IResultExporter` в `Application`), а реализуются снаружи, в `Infrastructure`.

## DDD

### Ubiquitous Language
Одни и те же слова используются в ТЗ, коде, сообщениях об ошибках и меню.

| Термин | В коде | Тип |
|---|---|---|
| студент | `Student` | Entity, корень агрегата |
| рубрикатор | `Rubric` | Entity, корень агрегата |
| критерий | `Criterion` | Entity внутри агрегата `Rubric` |
| оценка | `Evaluation` | Entity, корень агрегата |
| балл | `Score` | Value Object |
| максимум критерия | `MaxScore` | Value Object |
| вес (вклад в итог) | `Weight` | Value Object |
| ФИО студента | `PersonName` | Value Object |
| итог | `FinalGrade` | Value Object, только вычисляется |
| правило итога | `IFinalGradeRule` | Strategy |

### Entity и Value Object
**Entity** имеет идентичность, которая не меняется при изменении полей. Например, `Student`: после `Rename` это тот же студент (тот же `StudentId`), а два студента с одинаковым именем — разные люди. Равенство сущностей по `Id` реализовано в базовом классе `Entity<TId>`.

**Value Object** не имеет идентичности, неизменяем и сравнивается по значению: `Score.Of(5) == Score.Of(5)`. VO сам проверяет себя при создании (`Score.Of(-1)` бросает исключение), поэтому некорректный балл, вес или имя просто не могут существовать. Идентификаторы (`StudentId`, `CriterionId`, ...) — тоже value objects: их нельзя случайно перепутать друг с другом, как обычные `Guid`.

### Агрегаты
- `Rubric` + её `Criterion`: критерий нельзя создать или изменить в обход рубрикатора (конструктор и методы изменения у `Criterion` — `internal`).
- `Student` — отдельный небольшой агрегат. Оценки ему не принадлежат: они меняются независимо от студента.
- `Evaluation` — отдельный агрегат, ссылается на студента и критерий только по id. Конструктор `internal`, создать оценку можно только через `Rubric.CreateEvaluation`, потому что только рубрикатор знает, какие критерии существуют.
- Итог не хранится ни в одном агрегате: он вычисляется по запросу и поэтому всегда согласован с текущими оценками.

### Инварианты
Правила охраняются самой моделью, сценарии Application их не дублируют.

| Правило | Где охраняется | Исключение | Тесты |
|---|---|---|---|
| балл не выходит за пределы критерия | `Score.Of` (≥ 0), `Criterion.EnsureAccepts` (≤ max), вызывается из `Rubric.CreateEvaluation` и `Evaluation.Revise` | `InvalidValueException`, `ScoreOutOfRangeException` | `RubricTests`, `EvaluationTests`, `EvaluationServiceTests` |
| оценка только по критерию из рубрикатора | `Rubric.CreateEvaluation` через `GetCriterion` | `CriterionNotInRubricException` | `RubricTests`, `EvaluationServiceTests`, `FinalGradeCalculatorTests` |
| итог согласован с текущими оценками | итог не хранится, его считает `FinalGradeCalculator`; результат в пределах 0..100 | `EmptyRubricException`, `InvalidValueException` | `FinalGradeCalculatorTests`, `FacadeScenarioTests` |
| max > 0, вес > 0, имена не пустые | фабрики VO `MaxScore.Of`, `Weight.Of`, `PersonName.Of`, проверка имени в `Criterion` | `InvalidValueException` | `ValueObjectTests`, `RubricTests` |
| имена критериев уникальны в рубрикаторе | `Rubric.AddCriterion`, `Rubric.UpdateCriterion` | `DuplicateCriterionException` | `RubricTests`, `RubricServiceTests` |

Правила, которые затрагивают **несколько агрегатов**, проверяются в сценариях Application: уникальность имени студента, одна оценка на пару (студент, критерий), каскадное удаление оценок при удалении студента или критерия, запрет переопределять рубрикатор, пока есть оценки. Ни один отдельный агрегат не видит всех нужных данных, поэтому это сознательное решение.

### Repository
Порты `IStudentRepository`, `IRubricRepository`, `IEvaluationRepository` объявлены в `Domain` (по одному на корень агрегата), реализации `InMemory*Repository` лежат в `Infrastructure/Persistence`. Домен не знает, где и как хранятся данные.

### Domain Service
`FinalGradeCalculator` — итог зависит сразу от рубрикатора и от оценок студента, поэтому не принадлежит ни одной сущности. Сервис сопоставляет оценки с критериями (заодно проверяя, что каждая оценка относится к критерию из рубрикатора и что на критерий не больше одной оценки), передаёт результат правилу итога, округляет до сотых и возвращает `FinalGrade`.

Правила итога:
- `all-criteria`: `итог = Σ(вес · балл / максимум) / Σ вес · 100%`, критерий без оценки даёт 0 (итог курса);
- `graded-only`: та же формула только по оценённым критериям (текущая успеваемость).

## Паттерны GoF

| Группа | Паттерн | Где | Какую боль закрывает |
|---|---|---|---|
| Порождающий | **Builder** | `Pos.Domain/Rubrics/RubricBuilder.cs`; используется в `RubricService.Define`, `InMemoryRubricRepository`, тестах | Рубрикатор — составной объект (название + N критериев с максимумом и весом), который не должен существовать в невалидном или частично собранном виде. Builder собирает части по шагам, а `Build()` — единственная точка создания, где проверяются все инварианты. `Rubric.Create` — `internal`, поэтому обойти Builder нельзя. Если хотя бы один критерий некорректен, рубрикатор не создаётся и не сохраняется. |
| Структурный | **Facade** | `Pos.Application/Facade/IPosFacade.cs`, `PosFacade.cs` | Консоли нужны сценарии из четырёх сервисов, но без фасада каждый экран и демо зависели бы от нескольких сервисов, а сценарии оказались бы «размазаны» по UI. `IPosFacade` — единственная точка входа в Application для UI: экраны и демо знают только его и DTO |
| Поведенческий | **Strategy** | `Pos.Domain/Grading/IFinalGradeRule.cs`, `AllCriteriaRule`, `GradedOnlyRule`; используется в `FinalGradeCalculator` | Правило итога меняется (итог курса или текущая успеваемость), а сопоставление оценок с критериями и проверки остаются общими. Правило выбирается пользователем при просмотре и экспорте итога. Новое правило (например, «без худшей оценки») = новый класс и одна строка регистрации в DI, без правок домена и UI |

## Решения, принятые в ходе проектирования
- в центре решения — домен без внешних зависимостей; консоль, хранилище и форматы экспорта можно заменить, не трогая доменную модель и сценарии
- доменные объекты не выходят за пределы Application: UI получает только DTO и работает с id в виде `Guid`
- ошибки разделены на доменные (`DomainException` — нарушено правило модели) и ошибки сценариев (`UseCaseException` — не найдено, конфликт). Обе группы перехватываются в одном месте (`PosConsoleApp.Safely`) и показываются сообщением
- `MaxScore` у критерия нельзя изменить после создания: иначе уже выставленные оценки могли бы молча стать некорректными. Название и вес менять можно
- текущее время приходит через `TimeProvider`, а не через `DateTime.Now`: дата оценки передаётся в домен снаружи, и домен не зависит от системных часов
- `ConsoleIO` читает и пишет через внедрённые `TextReader` / `TextWriter`, поэтому меню и демо тестируются со сценарием ввода в строке
- хранилище — в памяти: данные живут до закрытия приложения. Файловое хранилище добавляется новым адаптером (см. ниже)

## DI
Контейнер `ServiceCollection` из `Microsoft.Extensions.DependencyInjection`. Каждый слой предоставляет свой метод регистрации (`AddPosApplication`, `AddPosInfrastructure`, `AddPosConsole`), а граф собирается в одном месте — Composition Root в `Program.cs`. Используется только внедрение через конструктор, без service locator. `ValidateOnBuild` проверяет все регистрации при старте.

**Жизненные циклы в DI:**
- репозитории — Singleton, потому что хранят данные всей сессии;
- `FinalGradeCalculator`, правила итога, экспортёры, `TimeProvider` — Singleton, потому что не имеют состояния;
- `ConsoleIO` — Singleton, один на консоль;
- сервисы Application, фасад, экраны, `DemoScenario` — Transient: состояния у них нет, всё состояние в репозиториях.

Те же методы регистрации используются в тестах (`TestHost`), только вместо `Console.In` / `Console.Out` подставляются `StringReader` / `StringWriter`.

## Тесты
`tests/Pos.Tests`, папки повторяют слои:
- `Domain` — value objects, `Student`, `Rubric` и `RubricBuilder`, `Evaluation`, `FinalGradeCalculator` с обоими правилами (без моков);
- `Application` — сервисы с репозиториями, подменёнными через **Moq**: проверяется и результат, и то, что при нарушении правила ничего не сохраняется;
- `Infrastructure` — экспорт в CSV/JSON и in-memory репозитории;
- `EndToEnd` — сквозной сценарий через фасад на реальном графе из DI и прогон консольного меню и демо со сценарием ввода.

## Расширение решения
- **Новое правило итога** (округление, «без худшей оценки», зачёт/незачёт) — новая реализация `IFinalGradeRule` и строка в `AddPosApplication`. Домен, сервисы и UI не меняются: пункт появится в списке правил автоматически.
- **Новый формат экспорта** — новая реализация `IResultExporter` в `Infrastructure` и строка в `AddPosInfrastructure`.
- **Хранение в файлах или БД** — новые реализации трёх репозиториев в `Infrastructure/Persistence`, в DI меняются три строки.
- **Несколько курсов** — у `Evaluation` появляется ссылка на рубрикатор, у `IRubricRepository` — поиск по `RubricId` вместо `GetCurrent()`.
- **Пересдачи и апелляции** — новый агрегат или история изменений `Evaluation`; правило итога уже вынесено в Strategy.
- **Другой UI** (веб, GUI) работает через тот же `IPosFacade`.

## Использование ИИ в работе
### Идеология
**Ответственность программиста охватывает**
- архитектуру, design
- глобальные решения по проектированию
- представление предметной области в абстракциях (иерархии, интерфейсы)
- реализацию ключевой бизнес-логики
- взаимодействие компонентов


**На ИИ-агента можно переложить**
- генерацию однотипного кода (например, в смежных классах иерархии)
- генерацию простого, но объемного кода (например, объемный вывод при консольном взаимодействии)
- генерацию расширенных тестовых случаев, размножение тестовых случаев
- повышение процента покрытия тестами
- прочую монотонную работу с кодом - поддержание codestyle, naming, дополнительное комментирование

### No code: 
| Факт использования | Модель |
| :--- | :--- |
| Обсуждение вопросов структуризации решения (разбиение на папки и файлы проекта на начальной стадии) | Claude Sonnet 5.5 |
| Проверка решения на соответствие установленным в задании требованиям | Claude Opus 5.5 |

### Code:
| Факт использования | Модель |
| :--- | :--- |
| Создание однотипных классов по заданному образцу | Claude Sonnet 5.5 |
| Создание расширенных тестовых случаев, увеличение процента покрытия тестами (Tests) | Claude Sonnet/Opus 5.5 |
| Разработка консольного интерфейса (оболочка для взаимодействия с бизнес-логикой) (`Console`) | Claude Sonnet/Opus 5.5 |
| Внесение общих незначительных правок в код (naming, codestyle, модификаторы) | Claude Sonnet/Opus 5.5 |
| Наполнение комментариями и документирование в коде | Claude Opus 5.5 |
| Агрегация различных данных из кода (наименования тестов, зависимостей, названия файлов и проч.) с последующей подстановкой в README (этот отчет)| Claude Opus 5.5 |

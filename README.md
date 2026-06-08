# Blazor1 / Vet52

## Назначение

Система предназначена для регистрации, хранения и обработки донесений об особо опасных болезнях животных. Приложение позволяет ветеринарным специалистам создавать, редактировать и удалять донесения, просматривать историю всех поступивших отчётов, а также экспортировать данные в документы Word и получать email-уведомления о новых донесениях.

---

## Стек технологий

- **ASP.NET Core Blazor Server** — фреймворк для разработки веб-приложений на C# с server-side rendering
- **Entity Framework Core 6.0** — ORM для работы с базой данных
- **Microsoft SQL Server / LocalDB** — СУБД для хранения данных
- **ASP.NET Core Identity** — встроенная система аутентификации и управления пользователями
- **MailKit** — библиотека для отправки email через SMTP
- **DocumentFormat.OpenXml** — работа с документами Word (.docx)
- **Bootstrap 5** — CSS фреймворк для отзывчивого дизайна
- **Git** — система контроля версий

---

## Реализованные функции

✅ **Авторизация пользователей**
- Регистрация новых пользователей
- Требуемое подтверждение аккаунта
- Безопасная аутентификация через ASP.NET Core Identity

✅ **Управление донесениями (CRUD)**
- **Создание** — форма для ввода информации о болезни с валидацией
- **Просмотр** — таблица со всеми донесениями, отсортированная по дате
- **Редактирование** — изменение существующего донесения
- **Удаление** — удаление с подтверждением через JavaScript

✅ **Экспорт донесения в Word**
- Документ создаётся на лету с форматированием
- Заголовок: **жирный, по центру, увеличенный размер**
- Все поля отображаются с пробелами после двоеточий
- Пустые поля показываются как "не указано"
- Файл автоматически скачивается браузером

✅ **Email-уведомления**
- Подготовлена отправка письма после создания нового донесения
- Письмо содержит все данные донесения (дата, район, болезнь, животные, место, описание, ответственный)
- Поддержка двух портов: 587 (STARTTLS) и 465 (SMTPS)
- Timeout 10 секунд — приложение не зависнет при недоступности SMTP
- Пароль хранится в user-secrets, не в репозитории

---

## Основные страницы

| Маршрут | Описание |
|---------|---------|
| `/` | Главная страница (перенаправление на список) |
| `/disease-reports` | Список всех донесений с действиями |
| `/disease-reports/create` | Форма создания нового донесения |
| `/disease-reports/edit/{id}` | Форма редактирования существующего донесения |

---

## Основные файлы проекта

### Models
- **`Models/DiseaseReport.cs`** — модель данных для донесения
  - Поля: Id, District, ReportDate, DiseaseName, AnimalType, SickCount, DeadCount, Location, Description, ResponsiblePerson, CreatedAt, CreatedBy
  - Display атрибуты на русском языке
  - Валидация обязательных полей

### Data
- **`Data/ApplicationDbContext.cs`** — контекст Entity Framework Core
  - Наследует IdentityDbContext для интеграции с ASP.NET Core Identity
  - DbSet<DiseaseReport> для работы с таблицей донесений

### Pages (Razor компоненты)
- **`Pages/DiseaseReports.razor`** — список донесений
  - Таблица со всеми отчётами
  - Кнопки: Редактировать, Экспортировать в Word, Удалить
  - Создание нового донесения
  - Подтверждение удаления через JavaScript

- **`Pages/CreateDiseaseReport.razor`** — создание нового донесения
  - EditForm с DataAnnotationsValidator
  - Поля формы соответствуют модели DiseaseReport
  - После сохранения: отправка email и вывод статуса
  - Форма сбрасывается после успешного сохранения

- **`Pages/EditDiseaseReport.razor`** — редактирование донесения
  - Загрузка существующих данных по Id
  - Обновление в базе после валидации

### Services
- **`Services/WordExportService.cs`** — генерация документов Word
  - Метод: `GenerateDiseaseReportDocument(DiseaseReport report)`
  - Создаёт .docx файл с форматированием
  - Использует DocumentFormat.OpenXml

- **`Services/EmailService.cs`** — отправка email-уведомлений
  - Метод: `SendDiseaseReportCreatedAsync(DiseaseReport report)`
  - Логирование всех операций SMTP
  - Timeout 10 секунд
  - Возвращает bool (успешность отправки)
  - Поддержка портов 587 (StartTls) и 465 (SslOnConnect)

- **`Services/EmailSettings.cs`** — конфигурация email
  - SmtpServer, SmtpPort, SenderName, SenderEmail
  - SenderPassword, RecipientEmail, EnableSsl

### Конфигурация
- **`appsettings.json`** — основные настройки приложения
  - ConnectionString для LocalDB
  - Примеры EmailSettings (без реального пароля)

- **`Program.cs`** — регистрация сервисов
  - DbContext, Identity, WordExportService, EmailService

---

## Как запустить проект

### 1. Установка зависимостей
```bash
dotnet restore
```

### 2. Создание и применение миграций БД
```bash
dotnet ef database update
```

Эта команда создаст LocalDB с таблицами Identity и DiseaseReports.

### 3. Запуск приложения
```bash
dotnet run
```

Приложение запустится на:
- **HTTPS:** https://localhost:7170
- **HTTP:** http://localhost:5280

---

## Как настроить email

Email отправляется через SMTP с использованием MailKit. Для работы с Gmail или другими SMTP-сервисами нужно настроить user-secrets.

### Инициализация user-secrets
```bash
dotnet user-secrets init
```

### Установка параметров email (пример для Gmail)

Используйте свои данные:

```bash
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SenderName" "Your Name"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SenderPassword" "your-app-password"
dotnet user-secrets set "EmailSettings:RecipientEmail" "recipient@example.com"
dotnet user-secrets set "EmailSettings:EnableSsl" "true"
```

**Важно:**
- Для Gmail используйте **App Password** (не основной пароль)
- Инструкция: https://support.google.com/accounts/answer/185833
- user-secrets хранится локально и не попадает в репозиторий
- Для production используйте Azure Key Vault или другие secure хранилища

### Проверка установленных секретов
```bash
dotnet user-secrets list
```

### Если нет переменных окружения
Если письма отправляться не будут, проверьте логи приложения — EmailService выведет подробное сообщение об ошибке и причину.

---

## Структура проекта

```
Blazor1/
├── Models/
│   └── DiseaseReport.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Migrations/
│   └── WeatherForecast.cs
├── Pages/
│   ├── DiseaseReports.razor
│   ├── CreateDiseaseReport.razor
│   ├── EditDiseaseReport.razor
│   ├── Index.razor
│   ├── Counter.razor
│   ├── FetchData.razor
│   └── _Host.cshtml
├── Services/
│   ├── WordExportService.cs
│   ├── EmailService.cs
│   └── EmailSettings.cs
├── Shared/
│   ├── NavMenu.razor
│   ├── MainLayout.razor
│   └── LoginDisplay.razor
├── wwwroot/
│   ├── css/
│   └── js/
│       └── downloadFile.js
├── Areas/
│   └── Identity/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── README.md
```

---

## Основные сценарии использования

### Сценарий 1: Создание нового донесения
1. Перейти на `/disease-reports`
2. Нажать кнопку "Новое донесение"
3. Заполнить форму
4. Нажать "Сохранить"
5. Донесение сохранится в БД, письмо отправится на email (если настроен)

### Сценарий 2: Экспорт донесения в Word
1. В списке донесений найти нужное
2. Нажать кнопку "Word" (blue button)
3. Браузер скачает файл `Донесение_[Район]_[Дата].docx`
4. Открыть в Microsoft Word или LibreOffice

### Сценарий 3: Удаление донесения
1. В списке донесений нажать кнопку "Удалить"
2. Подтвердить удаление в диалоговом окне
3. Запись удалится из БД

---

## Что можно доработать дальше

### Функциональность
- 🔐 **Роли пользователей** — разные права для администраторов, ветеринаров, просмотра
- 🔍 **Фильтрация** — поиск по района, дате, названию болезни
- 📋 **Шаблоны документов** — приказы, акты, справки в Word
- 📊 **Статистика** — графики, диаграммы, аналитика по болезням
- 📝 **Журнал действий** — логирование всех операций (создание, редактирование, удаление)
- 📧 **Группирование email** — рассылка нескольких донесений в одном письме

### Дизайн и UX
- 🎨 **Улучшение интерфейса** — современный дизайн, лучшая навигация
- 📱 **Адаптивность** — оптимизация для мобильных устройств
- 🌙 **Dark mode** — тёмная тема

### Масштабирование
- ☁️ **Миграция на Azure / AWS** — облачное развёртывание
- 🔄 **API** — REST API для интеграции с другими системами
- 📲 **Mobile приложение** — мобильное приложение для быстрого заполнения

### Безопасность
- 🔐 **2FA** — двухфакторная аутентификация
- 🛡️ **Шифрование данных** — шифрование чувствительных полей
- 🔒 **SSL сертификат** — HTTPS для production

---

## Разработка и тестирование

### Проверка синтаксиса и сборка
```bash
dotnet build
```

### Запуск с отладкой
```bash
dotnet run
```
Приложение запустится с hot reload — изменения в коде применяются автоматически.

### Применение миграций
```bash
dotnet ef migrations add "MigrationName"
dotnet ef database update
```

---

## Автор

Практическое задание по разработке информационной системы для Vet52 на ASP.NET Core Blazor Server.

---

## Лицензия

Проект создан в учебных целях.

---

## Контакты и поддержка

Для вопросов и предложений по улучшению проекта используйте:
- Issues в GitHub
- Email или прямое обращение к разработчикам команды

---

**Дата последнего обновления:** 08.06.2026

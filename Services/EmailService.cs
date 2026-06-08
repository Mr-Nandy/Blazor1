using System;
using Blazor1.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Blazor1.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendDiseaseReportCreatedAsync(DiseaseReport report)
        {
            try
            {
                _logger.LogInformation($"=== НАЧАЛО ОТПРАВКИ EMAIL ДЛЯ ДОНЕСЕНИЯ ID={report.Id} ===");

                // Проверка обязательных полей
                _logger.LogInformation("Проверка настроек EmailSettings...");

                if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
                {
                    _logger.LogWarning("❌ SmtpServer не настроен (пустое значение).");
                    return false;
                }

                if (_emailSettings.SmtpPort <= 0)
                {
                    _logger.LogWarning($"❌ SmtpPort некорректен: {_emailSettings.SmtpPort}. Должен быть > 0.");
                    return false;
                }

                if (string.IsNullOrEmpty(_emailSettings.SenderEmail))
                {
                    _logger.LogWarning("❌ SenderEmail не настроен (пустое значение).");
                    return false;
                }

                if (string.IsNullOrEmpty(_emailSettings.SenderPassword))
                {
                    _logger.LogWarning("❌ SenderPassword не настроен (пустое значение).");
                    return false;
                }

                if (string.IsNullOrEmpty(_emailSettings.RecipientEmail))
                {
                    _logger.LogWarning("❌ RecipientEmail не настроен (пустое значение).");
                    return false;
                }

                _logger.LogInformation("✓ Все настройки присутствуют:");
                _logger.LogInformation($"  - SmtpServer: {_emailSettings.SmtpServer}");
                _logger.LogInformation($"  - SmtpPort: {_emailSettings.SmtpPort}");
                _logger.LogInformation($"  - SenderEmail: {_emailSettings.SenderEmail}");
                _logger.LogInformation($"  - RecipientEmail: {_emailSettings.RecipientEmail}");
                _logger.LogInformation($"  - EnableSsl: {_emailSettings.EnableSsl}");

                // Формирование тела письма
                _logger.LogInformation("Формирование тела письма...");
                var messageBody = BuildEmailBody(report);

                // Создание письма
                _logger.LogInformation("Создание MimeMessage...");
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", _emailSettings.RecipientEmail));
                message.Subject = "Новое донесение об особо опасной болезни животных";
                message.Body = new TextPart("plain")
                {
                    Text = messageBody
                };
                _logger.LogInformation($"✓ Письмо создано. Тема: '{message.Subject}'");

                // Отправка письма с timeout 10 секунд
                _logger.LogInformation($"Подключение к SMTP серверу {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}...");
                
                // Определение режима подключения в зависимости от порта
                SecureSocketOptions secureSocketOptions = _emailSettings.SmtpPort switch
                {
                    587 => SecureSocketOptions.StartTls,
                    465 => SecureSocketOptions.SslOnConnect,
                    _ => SecureSocketOptions.Auto
                };
                
                _logger.LogInformation($"Режим подключения: {(_emailSettings.SmtpPort == 465 ? "SslOnConnect (порт 465)" : _emailSettings.SmtpPort == 587 ? "StartTls (порт 587)" : $"Auto (порт {_emailSettings.SmtpPort})")}");

                // Timeout 10 секунд для всех SMTP операций
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                using (var client = new SmtpClient())
                {
                    try
                    {
                        _logger.LogInformation("Вызов ConnectAsync с timeout 10 сек...");
                        await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, secureSocketOptions, cts.Token);
                        _logger.LogInformation($"✓ Успешное подключение к {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");

                        _logger.LogInformation($"Авторизация с email: {_emailSettings.SenderEmail}...");
                        await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword, cts.Token);
                        _logger.LogInformation($"✓ Успешная авторизация");

                        _logger.LogInformation("Отправка письма...");
                        await client.SendAsync(message, cts.Token);
                        _logger.LogInformation($"✓ Письмо успешно отправлено");

                        _logger.LogInformation("Отключение от SMTP...");
                        await client.DisconnectAsync(true, cts.Token);
                        _logger.LogInformation("✓ Отключено от SMTP");
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogError($"❌ TIMEOUT: Операция SMTP превысила 10 секунд и была отменена. Сервер: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                        return false;
                    }
                }

                _logger.LogInformation($"=== EMAIL УСПЕШНО ОТПРАВЛЕН ДЛЯ ДОНЕСЕНИЯ ID={report.Id} ===\n");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ОШИБКА при отправке email для донесения ID={report.Id}:");
                _logger.LogError($"   Тип ошибки: {ex.GetType().Name}");
                _logger.LogError($"   Сообщение: {ex.Message}");
                _logger.LogError($"   StackTrace: {ex.StackTrace}\n");
                return false;
            }
        }

        private string BuildEmailBody(DiseaseReport report)
        {
            var body = new System.Text.StringBuilder();
            body.AppendLine("Было создано новое донесение об особо опасной болезни животных.\n");
            body.AppendLine($"Дата донесения: {report.ReportDate.ToShortDateString()}");
            body.AppendLine($"Район: {report.District}");
            body.AppendLine($"Болезнь: {report.DiseaseName}");
            body.AppendLine($"Вид животного: {report.AnimalType ?? "не указано"}");
            body.AppendLine($"Количество заболевших: {report.SickCount}");
            body.AppendLine($"Количество павших: {report.DeadCount}");
            body.AppendLine($"Место выявления: {report.Location ?? "не указано"}");
            body.AppendLine($"Описание ситуации: {(string.IsNullOrWhiteSpace(report.Description) ? "не указано" : report.Description)}");
            body.AppendLine($"Ответственный специалист: {report.ResponsiblePerson ?? "не указано"}");
            body.AppendLine($"Дата создания: {report.CreatedAt.ToString("dd.MM.yyyy HH:mm")}");
            return body.ToString();
        }
    }
}

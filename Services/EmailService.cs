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

        public async Task SendDiseaseReportCreatedAsync(DiseaseReport report)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrEmpty(_emailSettings.RecipientEmail))
                {
                    _logger.LogWarning("Адрес получателя email не настроен. Отправка пропущена.");
                    return;
                }

                if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
                {
                    _logger.LogWarning("SMTP сервер не настроен. Отправка пропущена.");
                    return;
                }

                // Формирование тела письма
                var messageBody = BuildEmailBody(report);

                // Создание письма
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", _emailSettings.RecipientEmail));
                message.Subject = "Новое донесение об особо опасной болезни животных";
                message.Body = new TextPart("plain")
                {
                    Text = messageBody
                };

                // Отправка письма
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email успешно отправлен для донесения ID={report.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при отправке email для донесения ID={report.Id}: {ex.Message}");
                // Не прерываем выполнение, так как запись в БД уже сохранена
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

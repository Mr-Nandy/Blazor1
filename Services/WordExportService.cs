using Blazor1.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Blazor1.Services
{
    public class WordExportService
    {
        public byte[] GenerateDiseaseReportDocument(DiseaseReport report)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {
                    var mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = mainPart.Document.Body ?? new Body();
                    mainPart.Document.Append(body);

                    // Заголовок
                    var titleParagraph = CreateParagraph("Донесение об особо опасной болезни животных", isBold: true);
                    body.Append(titleParagraph);

                    // Пустая строка
                    body.Append(new Paragraph());

                    // Содержание
                    body.Append(CreateFieldParagraph("Дата донесения:", report.ReportDate.ToShortDateString()));
                    body.Append(CreateFieldParagraph("Район:", report.District));
                    body.Append(CreateFieldParagraph("Болезнь:", report.DiseaseName));
                    body.Append(CreateFieldParagraph("Вид животного:", report.AnimalType ?? "-"));
                    body.Append(CreateFieldParagraph("Количество заболевших:", report.SickCount.ToString()));
                    body.Append(CreateFieldParagraph("Количество павших:", report.DeadCount.ToString()));
                    body.Append(CreateFieldParagraph("Место выявления:", report.Location ?? "-"));
                    body.Append(CreateFieldParagraph("Описание ситуации:", report.Description ?? "-"));
                    body.Append(CreateFieldParagraph("Ответственный специалист:", report.ResponsiblePerson ?? "-"));
                    body.Append(CreateFieldParagraph("Дата создания:", report.CreatedAt.ToString("dd.MM.yyyy HH:mm")));
                }

                return memoryStream.ToArray();
            }
        }

        private static Paragraph CreateParagraph(string text, bool isBold = false)
        {
            var run = new Run(new Text(text));
            
            if (isBold)
            {
                var runProperties = new RunProperties();
                runProperties.Append(new Bold());
                run.PrependChild(runProperties);
            }

            return new Paragraph(run);
        }

        private static Paragraph CreateFieldParagraph(string label, string value)
        {
            var labelRun = new Run(new RunProperties(new Bold()), new Text(label + " "));
            var valueRun = new Run(new Text(value));
            
            var paragraph = new Paragraph(labelRun, valueRun);
            
            return paragraph;
        }
    }
}

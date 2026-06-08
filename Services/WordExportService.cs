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

                    // Заголовок - жирный, по центру, крупнее
                    var titleParagraph = CreateTitleParagraph("Донесение об особо опасной болезни животных");
                    body.Append(titleParagraph);

                    // Пустая строка
                    body.Append(new Paragraph());

                    // Содержание - каждое поле на новой строке
                    body.Append(CreateFieldParagraph("Дата донесения:", report.ReportDate.ToShortDateString()));
                    body.Append(CreateFieldParagraph("Район:", report.District));
                    body.Append(CreateFieldParagraph("Болезнь:", report.DiseaseName));
                    body.Append(CreateFieldParagraph("Вид животного:", report.AnimalType ?? "не указано"));
                    body.Append(CreateFieldParagraph("Количество заболевших:", report.SickCount.ToString()));
                    body.Append(CreateFieldParagraph("Количество павших:", report.DeadCount.ToString()));
                    body.Append(CreateFieldParagraph("Место выявления:", report.Location ?? "не указано"));
                    body.Append(CreateFieldParagraph("Описание ситуации:", string.IsNullOrWhiteSpace(report.Description) ? "не указано" : report.Description));
                    body.Append(CreateFieldParagraph("Ответственный специалист:", report.ResponsiblePerson ?? "не указано"));
                    body.Append(CreateFieldParagraph("Дата создания:", report.CreatedAt.ToString("dd.MM.yyyy HH:mm")));
                }

                return memoryStream.ToArray();
            }
        }

        private static Paragraph CreateTitleParagraph(string text)
        {
            // Жирный, крупнее (24pt), по центру
            var runProperties = new RunProperties();
            runProperties.Append(new Bold());
            runProperties.Append(new FontSize { Val = "48" }); // 48 half-points = 24pt

            var run = new Run(runProperties, new Text(text));

            var paragraphProperties = new ParagraphProperties();
            paragraphProperties.Append(new Justification { Val = JustificationValues.Center });

            var paragraph = new Paragraph(paragraphProperties, run);

            return paragraph;
        }

        private static Paragraph CreateFieldParagraph(string label, string value)
        {
            var labelRun = new Run(new RunProperties(new Bold()), new Text(label + " ") { Space = SpaceProcessingModeValues.Preserve });
            var valueRun = new Run(new Text(value));

            var paragraph = new Paragraph(labelRun, valueRun);

            return paragraph;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Blazor1.Models
{
    public class DiseaseReport
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Район обязателен")]
        [Display(Name = "Район")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Дата донесения обязательна")]
        [Display(Name = "Дата донесения")]
        public DateTime ReportDate { get; set; }

        [Required(ErrorMessage = "Название болезни обязательно")]
        [Display(Name = "Название болезни")]
        public string DiseaseName { get; set; } = string.Empty;

        [Display(Name = "Вид животного")]
        public string? AnimalType { get; set; }

        [Display(Name = "Количество больных животных")]
        public int SickCount { get; set; }

        [Display(Name = "Количество павших животных")]
        public int DeadCount { get; set; }

        [Display(Name = "Место обнаружения")]
        public string? Location { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Ответственное лицо")]
        public string? ResponsiblePerson { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Создано")]
        public string? CreatedBy { get; set; }
    }
}

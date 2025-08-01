using System.ComponentModel.DataAnnotations;

namespace MeterReadingLibrary.Models;

internal class MeterReadingModel
{
    public int MeterReadingId { get; set; }
    public int AccountId { get; set; }

    [Required]
    public DateTime MeterReadingDate { get; set; }

    [Required]
    public int MeterReadingValue { get; set; }
}
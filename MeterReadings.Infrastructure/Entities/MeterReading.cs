using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterReadings.Infrastructure.Entities
{
    [PrimaryKey(nameof(MeterReadingId))]
    [Index(nameof(AccountId))]
    public class MeterReading
    {
        [Required]
        public int MeterReadingId { get; set; }
        [Required]
        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }
        public virtual Account? Account { get; set; }
        [Required]
        public DateTime MeterReadingDateTime { get; set; }
        [Required]
        [MaxLength(5)]
        public string? MeterReadValue { get; set; }
    }
}

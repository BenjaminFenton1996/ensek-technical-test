using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterReadings.Infrastructure.Entities
{
    [PrimaryKey(nameof(MeterReadingId))]
    public class MeterReading
    {
        public required int MeterReadingId { get; set; }
        [ForeignKey(nameof(Account.AccountId))]
        public required Account Account { get; set; }
        public required DateTime MeterReadingDateTime { get; set; }
        public required int MeterReadValue { get; set; }
    }
}

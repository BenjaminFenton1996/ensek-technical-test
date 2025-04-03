using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Infrastructure.Entities
{
    [PrimaryKey(nameof(AccountId))]
    public class Account
    {
        public required int AccountId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}

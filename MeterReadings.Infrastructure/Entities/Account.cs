using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MeterReadings.Infrastructure.Entities
{
    [PrimaryKey(nameof(AccountId))]
    [Index(nameof(AccountId))]
    public class Account
    {
        [Required]
        public int AccountId { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
    }
}

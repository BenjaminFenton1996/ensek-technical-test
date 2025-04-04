using System.ComponentModel.DataAnnotations;

namespace MeterReadings.Infrastructure.Entities
{
    internal static class EntityValidation
    {
        public static bool IsValid(object entity)
        {
            var validationContext = new ValidationContext(entity);
            return Validator.TryValidateObject(entity, validationContext, null, true);
        }
    }
}

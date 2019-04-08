using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Attributes
{
    public class PositiveNumberAttribute : ValidationAttribute
    {
        public PositiveNumberAttribute()
        {
            ErrorMessage = "Value must be positive";
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            if (decimal.TryParse(value.ToString(), out var num))
                if (num > 0)
                    return true;
            return false;
        }
    }
}
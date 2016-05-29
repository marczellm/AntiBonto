using System;
using System.Globalization;
using System.Windows.Controls;

namespace AntiBonto.View
{
    class IntValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string)
            {
                int number;
                if (!Int32.TryParse((value as string), out number))
                    return new ValidationResult(false, "");
            }

            return ValidationResult.ValidResult;
        }
    }
}

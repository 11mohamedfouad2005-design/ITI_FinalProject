using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ITI_FinalProject.Validation
{
    public sealed class MinimumStudentAgeAttribute(int minimumAge) : ValidationAttribute, IClientModelValidator
    {
        public override bool IsValid(object? value)
        {
            if (value is not DateTime dateOfBirth)
            {
                return false;
            }

            return dateOfBirth.Date <= DateTime.Today.AddYears(-minimumAge);
        }

        public override string FormatErrorMessage(string name) =>
            $"{name} must be at least {minimumAge} years ago.";

        public void AddValidation(ClientModelValidationContext context)
        {
            AddIfMissing(context.Attributes, "data-val", "true");
            AddIfMissing(context.Attributes, "data-val-minimumstudentage", ErrorMessage ?? FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
            AddIfMissing(context.Attributes, "data-val-minimumstudentage-years", minimumAge.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private static void AddIfMissing(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key))
            {
                attributes.Add(key, value);
            }
        }
    }
}
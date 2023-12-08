using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace Barunson.FamilyWeb.Attribute
{
    public class MustbetrueAttribute : ValidationAttribute, IClientModelValidator
    {
        public MustbetrueAttribute() 
        {
            const string defaultErrorMessage = "필수 입력입니다.";
            ErrorMessage ??= defaultErrorMessage;
        }

        public override bool IsValid(object? value)
        {
            return value != null && value is bool && (bool)value;
        }
        

        public void AddValidation(ClientModelValidationContext context)
        {
            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-mustbetrue", errorMessage);
        }
                
        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }

            attributes.Add(key, value);
            return true;
        }
    }
}

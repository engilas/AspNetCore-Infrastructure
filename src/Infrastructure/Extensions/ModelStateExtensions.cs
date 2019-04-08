using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Infrastructure.Extensions
{
    public static class ModelStateExtensions
    {
        public static Dictionary<string, string> ToDictionary(this ModelStateDictionary modelState)
        {
            var result = new Dictionary<string, string>();
            foreach (var keyValue in modelState)
            {
                var key = keyValue.Key;
                var value = string.Join(", ",
                    keyValue.Value.Errors.Select(x => string.Join(", ",
                        new[] {x.ErrorMessage, x.Exception?.Message}.Where(m => !string.IsNullOrWhiteSpace(m)))));
                result.Add(key, value);
            }

            return result;
        }

        public static string ConvertToString(this ModelStateDictionary modelState)
        {
            var errors = modelState.ToDictionary();
            return string.Join(";",
                errors.Select(x => string.Join(": ", new[] {x.Key, x.Value}.Where(m => !string.IsNullOrWhiteSpace(m))))
                    .ToArray());
        }
    }
}
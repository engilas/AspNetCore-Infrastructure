using System.Collections.Generic;
using System.Reflection;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;

namespace Infrastructure.Configuration
{
    public abstract class OptionsBase : IOptionsValidation
    {
        /// <summary>
        ///     If validation failed, method should throw <see cref="OptionsValidationException" />
        /// </summary>
        public abstract void Validate();

        protected void ValidateEmptyString(string parameterName, string value)
        {
            parameterName.ThrowIfNullOrWhitespace(nameof(parameterName));
            if (string.IsNullOrWhiteSpace(value))
                throw new OptionsValidationException(GetType().Name, parameterName, "value is empty");
        }

        protected void ValidateNegativeValue(string parameterName, int value)
        {
            parameterName.ThrowIfNullOrWhitespace(nameof(parameterName));
            if (value < 0)
                throw new OptionsValidationException(GetType().Name, parameterName, "value is negative");
        }

        protected void ValidateNotPositiveValue(string parameterName, int value)
        {
            parameterName.ThrowIfNullOrWhitespace(nameof(parameterName));
            if (value <= 0)
                throw new OptionsValidationException(GetType().Name, parameterName, "value is not positive");
        }

        protected void ValidateNull(string parameterName, object value)
        {
            parameterName.ThrowIfNullOrWhitespace(nameof(parameterName));
            if (value == null)
                throw new OptionsValidationException(GetType().Name, parameterName, "value is null");
        }

        protected void NotifyInvalidValue(string reason, string parameterName)
        {
            throw new OptionsValidationException(GetType().Name, parameterName, reason);
        }

        protected void ValidateEmptyCollection<T>(string parameterName, IEnumerable<T> value)
        {
            parameterName.ThrowIfNullOrWhitespace(nameof(parameterName));
            if (!value.HasElements())
                throw new OptionsValidationException(GetType().Name, parameterName, "value has no elements");
        }

        protected void ValidateEmptyStrings()
        {
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in props)
            {
                if (propertyInfo.PropertyType == typeof(string))
                    ValidateEmptyString(propertyInfo.Name, (string) propertyInfo.GetValue(this));
            }
        }

        protected void ValidateEmptyStrings(object obj)
        {
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in props)
            {
                if (propertyInfo.PropertyType == typeof(string))
                    ValidateEmptyString(propertyInfo.Name, (string) propertyInfo.GetValue(obj));
            }
        }
    }
}
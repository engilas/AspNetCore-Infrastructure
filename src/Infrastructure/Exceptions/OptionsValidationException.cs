using System;

namespace Infrastructure.Exceptions
{
    public class OptionsValidationException : Exception
    {
        public OptionsValidationException(string optionsName, string parameterName, string validationError)
            : base($"Validation error in options {optionsName} of parameter {parameterName}: {validationError}")
        {
        }

        public OptionsValidationException(string optionsName, string validationError)
            : base($"Validation error in options {optionsName}: {validationError}")
        {
        }
    }
}
using Infrastructure.Exceptions;

namespace Infrastructure.Configuration
{
    public interface IOptionsValidation
    {
        /// <summary>
        ///     Method should thows <see cref="OptionsValidationException" /> if validation fails
        /// </summary>
        void Validate();
    }
}
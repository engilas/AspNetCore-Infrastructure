using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Infrastructure.Attributes
{
    public class CustomExceptionFilterAttribute : Attribute, IExceptionFilter
    {
        private readonly Type _handlerType;

        public CustomExceptionFilterAttribute(Type handlerType)
        {
            if (!typeof(IExceptionFilter).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException($"Type {handlerType.Name} must implement IExceptionFilter interface");
            }

            if (handlerType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public,
                    null, Type.EmptyTypes, null) == null)
            {
                throw new ArgumentException($"Type {handlerType.Name} has no parameterless constructor");
            }

            _handlerType = handlerType;
        }

        public void OnException(ExceptionContext context)
        {
            var handler = (IExceptionFilter) Activator.CreateInstance(_handlerType);
            handler.OnException(context);
        }
    }
}

using System;

namespace Infrastructure.Attributes
{
    /// <summary>
    ///     Say default api invalid model state handler don't generate BaseResponse with status code 200, status code 400 will
    ///     be returned.
    /// </summary>
    public class DontWrapInvalidModelStateAttribute : Attribute
    {
    }
}
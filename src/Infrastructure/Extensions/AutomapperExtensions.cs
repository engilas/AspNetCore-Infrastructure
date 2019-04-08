using AutoMapper;

namespace Infrastructure.Extensions
{
    public static class AutomapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreUnmapped<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());

            return expression;
        }

        public static T MapTo<T>(this object obj)
        {
            return Mapper.Map<T>(obj);
        }
    }
}

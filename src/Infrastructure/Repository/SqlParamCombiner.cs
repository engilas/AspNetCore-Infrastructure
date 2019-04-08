using System.Collections.Generic;
using Dapper;
using Infrastructure.Extensions;

namespace Infrastructure.Repository
{
    internal class SqlParamCombiner : DynamicParameters, ISqlParamCombiner
    {
        public ISqlParamCombiner Add(object item)
        {
            item.ThrowIfNullArgument(nameof(item));
            AddDynamicParams(item);
            return this;
        }

        public ISqlParamCombiner Add(string key, object value)
        {
            key.ThrowIfNullOrWhitespace(nameof(key));
            var kvp = new KeyValuePair<string, object>(key, value);
            AddDynamicParams(new[] {kvp});

            return this;
        }

        public ISqlParamCombiner AddId(int id)
        {
            AddDynamicParams(new {id});
            return this;
        }
    }
}
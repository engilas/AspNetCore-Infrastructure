using System.Collections.Generic;
using System.Linq;
using Infrastructure.Configuration;
using Infrastructure.Exceptions;

namespace Infrastructure.Repository
{
    public class DbOptions : OptionsBase
    {
        public Dictionary<string, string> ConnectionStrings { get; set; }

        public string this[string index] => GetConnectionString(index);

        public string GetConnectionString()
        {
            return ConnectionStrings["default"];
        }

        public string GetConnectionString(string key)
        {
            return ConnectionStrings[key];
        }

        public override void Validate()
        {
            ValidateNull(nameof(ConnectionStrings), ConnectionStrings);
            if (!ConnectionStrings.Any())
            {
                throw new OptionsValidationException(nameof(DbOptions), nameof(ConnectionStrings),
                    "No connection strings");
            }
        }
    }

    public class DbOptions<T> : DbOptions
    {
    }
}
using System.Text.RegularExpressions;

namespace Infrastructure.Logging
{
    public class IgnoreOptions
    {
        public string Route { get; set; }
        public string[] IgnoreErrors { get; set; }

        public Regex RouteRegex { get; set; }
    }
}
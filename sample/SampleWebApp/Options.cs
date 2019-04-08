using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Configuration;

namespace SampleWebApp
{
    /// <summary>
    /// Sample options class
    /// </summary>
    public class Options : OptionsBase
    {
        public string SomeValue { get; set; }

        public override void Validate()
        {
            ValidateEmptyStrings();
        }
    }
}

using System.Collections.Generic;

namespace Infrastructure.Features
{
    internal class FeatureCollection
    {
        public FeatureCollection(IEnumerable<IFeature> features)
        {
            Features = features;
        }

        public IEnumerable<IFeature> Features { get; }
    }
}
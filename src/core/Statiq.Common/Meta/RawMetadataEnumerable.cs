using System.Collections;
using System.Collections.Generic;

namespace Statiq.Common
{
    internal class RawMetadataEnumerable : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly IMetadata _metadata;

        public RawMetadataEnumerable(IMetadata metadata)
        {
            _metadata = metadata;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.GetRawEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

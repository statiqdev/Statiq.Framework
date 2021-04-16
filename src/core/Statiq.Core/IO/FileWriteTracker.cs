using System.Collections.Concurrent;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Core
{
    internal class FileWriteTracker : IFileWriteTracker
    {
        private readonly ConcurrentDictionary<NormalizedPath, int> _currentWrites
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _previousWrites
            = new ConcurrentDictionary<NormalizedPath, int>();

        public void Reset()
        {
            _previousWrites = _currentWrites;
            _currentWrites.Clear();
        }

        public void AddWrite(NormalizedPath path, int hashCode) => _currentWrites[path] = hashCode;

        public IEnumerable<KeyValuePair<NormalizedPath, int>> CurrentWrites => _currentWrites.ToArray();

        public IEnumerable<KeyValuePair<NormalizedPath, int>> PreviousWrites => _previousWrites.ToArray();
    }
}

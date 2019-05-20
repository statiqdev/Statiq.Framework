using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Util;

namespace Wyam.Common.Documents
{
    public static class IDocumentExtensions
    {
        public static async Task<int> GetHashAsync(this IDocument document)
        {
            if (document == null)
            {
                return 0;
            }

            HashCode hash = default;
            using (Stream stream = await document.GetStreamAsync())
            {
                hash.Add(await Crc32.CalculateAsync(stream));
            }
            foreach (KeyValuePair<string, object> item in document.WithoutSettings)
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }

            return hash.ToHashCode();
        }
    }
}

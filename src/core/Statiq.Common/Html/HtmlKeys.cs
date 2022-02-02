using System;
using Statiq.Common;

// Use the old Statiq.Html namespace for backwards-compatibility
namespace Statiq.Html
{
    /// <summary>
    /// Metadata keys for use with the various HTML processing modules. This class
    /// is kept for backwards compatibility, use the strings in <see cref="Keys"/>
    /// instead going forward.
    /// </summary>
    public static class HtmlKeys
    {
        [Obsolete("Use Keys.Excerpt instead")]
        public const string Excerpt = nameof(Keys.Excerpt);

        [Obsolete("Use Keys.Excerpt instead")]
        public const string OuterHtml = nameof(Keys.Excerpt);

        [Obsolete("Use Keys.Excerpt instead")]
        public const string InnerHtml = nameof(Keys.Excerpt);

        [Obsolete("Use Keys.Excerpt instead")]
        public const string TextContent = nameof(Keys.Excerpt);

        [Obsolete("Use Keys.Excerpt instead")]
        public const string Headings = nameof(Keys.Excerpt);

        [Obsolete("Use Keys.Excerpt instead")]
        public const string HeadingId = nameof(Keys.Excerpt);

        [Obsolete("Use Keys.Excerpt instead")]
        public const string Level = nameof(Keys.Excerpt);
    }
}
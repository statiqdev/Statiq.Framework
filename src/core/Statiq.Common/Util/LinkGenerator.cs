using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// Helps generate normalized links.
    /// </summary>
    public static class LinkGenerator
    {
        /// <summary>
        /// Generates a normalized link given a path and other conditions.
        /// </summary>
        /// <param name="path">The path to get a link for.</param>
        /// <param name="host">The host for the link (or <c>null</c> to omit the host).</param>
        /// <param name="root">The root path for the link (or <c>null</c> for no root path).</param>
        /// <param name="scheme">The scheme for the link (or <c>null</c> for "http").</param>
        /// <param name="hidePages">An array of file names to hide (or <c>null</c> to not hide any files).</param>
        /// <param name="hideExtensions">An array of file extensions to hide (or <c>null</c> to not hide extensions or an empty array to hide all file extensions).</param>
        /// <param name="lowercase">Indicates that the link should be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="scheme"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="scheme"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <returns>A generated link.</returns>
        public static string GetLink(
            NormalizedPath path,
            string host,
            in NormalizedPath root,
            string scheme,
            IReadOnlyList<string> hidePages,
            IReadOnlyList<string> hideExtensions,
            bool lowercase,
            bool makeAbsolute = true)
        {
            string link = string.Empty;
            if (!path.IsNull)
            {
                // Remove index pages
                if (hidePages is object && path.FullPath != NormalizedPath.Slash
                    && hidePages.Any(x => x is object && path.FileName.Equals(x)))
                {
                    path = path.Parent;
                }

                // Special case if this is relative and we removed the entire path
                if (!makeAbsolute && path.IsNullOrEmpty)
                {
                    // The "." indicates to a browser to link to the current directory, which is what we want
                    return ".";
                }

                // Hide extensions
                if (hideExtensions is object
                    && (hideExtensions.Count == 0 || hideExtensions.Where(x => x is object).Select(x => x.StartsWith(NormalizedPath.Dot) ? x : NormalizedPath.Dot + x).Contains(path.Extension)))
                {
                    path = path.ChangeExtension(null);
                }

                // Collapse the link to a string
                link = path.FullPath;

                // Prepend a slash, but only if we want to make the path absolute
                if (makeAbsolute)
                {
                    if (string.IsNullOrWhiteSpace(link) || link == NormalizedPath.Dot)
                    {
                        link = NormalizedPath.Slash;
                    }
                    if (!link.StartsWith(NormalizedPath.Slash))
                    {
                        link = NormalizedPath.Slash + link;
                    }
                }
            }

            // Extract the fragment
            int fragmentIndex = link.IndexOf('#');
            string fragment = null;
            if (fragmentIndex > -1)
            {
                fragment = link.Substring(fragmentIndex);
                link = link.Substring(0, fragmentIndex);
            }

            // Extract the query
            int queryIndex = link.IndexOf('?');
            string query = null;
            if (queryIndex > -1)
            {
                query = link.Substring(queryIndex);
                link = link.Substring(0, queryIndex);
            }

            // If we're not making it absolute and it doesn't start with a slash, make sure to remove the slash when we're done,
            // otherwise collapse with the root path and combine them
            bool makeRelative = false;
            if (!makeAbsolute && (link.Length == 0 || link[0] != NormalizedPath.Slash[0]))
            {
                makeRelative = true;
            }
            else
            {
                string rootLink = root.IsNull ? string.Empty : root.FullPath;
                if (rootLink.EndsWith(NormalizedPath.Slash))
                {
                    rootLink = rootLink.Substring(0, rootLink.Length - 1);
                }
                link = rootLink + link;
            }

            // Add the host and convert to URI for escaping
            UriBuilder builder = new UriBuilder
            {
                Scheme = scheme ?? "http",
                Path = link,
                Query = query,
                Fragment = fragment
            };
            bool hasHost = false;
            if (!makeRelative && !string.IsNullOrWhiteSpace(host))
            {
                builder.Host = host;
                hasHost = true;
            }
            Uri uri = builder.Uri;
            string renderedLink = hasHost
                ? uri.AbsoluteUri
                : uri.GetComponents(UriComponents.Path | UriComponents.Query | UriComponents.Fragment, UriFormat.SafeUnescaped);

            // Remove the slash prefix if we have one
            if (makeRelative && renderedLink[0] == '/')
            {
                renderedLink = renderedLink.Substring(1);

                // If the link started with a dot, add it back in
                if (link.Length > 0 && link[0] == '.' && (renderedLink.Length == 0 || renderedLink[0] != '.'))
                {
                    renderedLink = "." + renderedLink;
                }
            }

            return lowercase ? renderedLink.ToLowerInvariant() : renderedLink;
        }

        /// <summary>
        /// Checks if a string contains an absolute URI with a "http" or "https" scheme and returns it if it does.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="absoluteUri">The resulting absolute URI.</param>
        /// <returns><c>true</c> if the string contains an absolute URI, <c>false</c> otherwise.</returns>
        public static bool TryGetAbsoluteHttpUri(string str, out string absoluteUri)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                // Return the actual URI if it's absolute
                if (Uri.TryCreate(str, UriKind.Absolute, out Uri uri)
                    && (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                        || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
                {
                    absoluteUri = uri.ToString();
                    return true;
                }
            }
            absoluteUri = null;
            return false;
        }
    }
}

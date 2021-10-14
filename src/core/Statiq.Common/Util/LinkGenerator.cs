using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// Helps generate normalized links.
    /// </summary>
    public class LinkGenerator : ILinkGenerator
    {
        // Cache link generator results
        private static readonly ConcurrentCache<GetLinkCacheKey, string> _links =
            new ConcurrentCache<GetLinkCacheKey, string>(false);

        private class GetLinkCacheKey : IEquatable<GetLinkCacheKey>
        {
            public NormalizedPath Path { get; set; }

            public string Host { get; set; }

            public NormalizedPath Root { get; set; }

            public string Scheme { get; set; }

            public string[] HidePages { get; set; }

            public string[] HideExtensions { get; set; }

            public bool Lowercase { get; set; }

            public bool MakeAbsolute { get; set; }

            public bool Equals(GetLinkCacheKey other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Path.Equals(other.Path)
                   && Host == other.Host
                   && Root.Equals(other.Root)
                   && Scheme == other.Scheme
                   && Equals(HidePages, other.HidePages)
                   && Equals(HideExtensions, other.HideExtensions)
                   && Lowercase == other.Lowercase
                   && MakeAbsolute == other.MakeAbsolute;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((GetLinkCacheKey)obj);
            }

            public override int GetHashCode() =>
                HashCode.Combine(Path, Host, Root, Scheme, HidePages, HideExtensions, Lowercase, MakeAbsolute);
        }

        /// <inheritdoc />
        public string GetLink(
            NormalizedPath path,
            string host,
            in NormalizedPath root,
            string scheme,
            string[] hidePages,
            string[] hideExtensions,
            bool lowercase,
            bool makeAbsolute = true) =>
            _links.GetOrAdd(
                new GetLinkCacheKey
                {
                    Path = path,
                    Host = host,
                    Root = root,
                    Scheme = scheme,
                    HidePages = hidePages,
                    HideExtensions = hideExtensions,
                    Lowercase = lowercase,
                    MakeAbsolute = makeAbsolute
                },
                key => GetLinkImplementation(
                    key.Path,
                    key.Host,
                    key.Root,
                    key.Scheme,
                    key.HidePages,
                    key.HideExtensions,
                    key.Lowercase,
                    key.MakeAbsolute));

        private static string GetLinkImplementation(
            NormalizedPath path,
            string host,
            in NormalizedPath root,
            string scheme,
            string[] hidePages,
            string[] hideExtensions,
            bool lowercase,
            bool makeAbsolute)
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
                    && (hideExtensions.Length == 0
                        || hideExtensions
                            .Where(x => x is object)
                            .Select(x => x.StartsWith(NormalizedPath.Dot) ? x : NormalizedPath.Dot + x)
                            .Contains(path.Extension)))
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

        /// <inheritdoc />
        public bool TryGetAbsoluteHttpUri(string str, out string absoluteUri)
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

        /// <inheritdoc />
        public string AddQueryAndFragment(string path, string queryAndFragment)
        {
            if (!string.IsNullOrEmpty(queryAndFragment))
            {
                // If we have a query and fragment, make sure it starts with ? or #
                if (queryAndFragment[0] == '?' || queryAndFragment[0] == '#')
                {
                    return path + queryAndFragment;
                }
                return path + "?" + queryAndFragment;
            }
            return path;
        }

        /// <inheritdoc />
        public NormalizedPath AddQueryAndFragment(NormalizedPath path, string queryAndFragment)
        {
            if (!string.IsNullOrEmpty(queryAndFragment))
            {
                // If we have a query and fragment, make sure it starts with ? or #
                if (queryAndFragment[0] == '?' || queryAndFragment[0] == '#')
                {
                    return new NormalizedPath(path.FullPath + queryAndFragment);
                }
                return new NormalizedPath(path.FullPath + "?" + queryAndFragment);
            }
            return path;
        }
    }
}
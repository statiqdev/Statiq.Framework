using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Azure
{
    /// <summary>
    /// Deploys output files to Azure App Service using a zip file.
    /// </summary>
    /// <category>Deployment</category>
    public class DeployAppService : MultiConfigModule
    {
        // Config keys
        private const string SiteName = nameof(SiteName);
        private const string Username = nameof(Username);
        private const string Password = nameof(Password);
        private const string Directory = nameof(Directory);
        private const string ZipPath = nameof(ZipPath);

        /// <summary>
        /// Deploys the output folder to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password)
            : this(siteName, username, password, Config.FromValue((DirectoryPath)null))
        {
        }

        /// <summary>
        /// Deploys a specified folder to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="directory">
        /// The directory containing the files to deploy (from the root folder, not the input folder).
        /// </param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password, Config<DirectoryPath> directory)
            : base(
                new Dictionary<string, IConfig>
                {
                    { SiteName, siteName ?? throw new ArgumentNullException(nameof(siteName)) },
                    { Username, username ?? throw new ArgumentNullException(nameof(username)) },
                    { Password, password ?? throw new ArgumentNullException(nameof(password)) },
                    { Directory, directory ?? throw new ArgumentNullException(nameof(directory)) }
                },
                false)
        {
        }

        /// <summary>
        /// Deploys a specified zip file to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="zipPath">The zip file to deploy.</param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password, Config<FilePath> zipPath)
            : base(
                new Dictionary<string, IConfig>
                {
                    { SiteName, siteName ?? throw new ArgumentNullException(nameof(siteName)) },
                    { Username, username ?? throw new ArgumentNullException(nameof(username)) },
                    { Password, password ?? throw new ArgumentNullException(nameof(password)) },
                    { ZipPath, zipPath ?? throw new ArgumentNullException(nameof(zipPath)) }
                },
                false)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Get the site name
            string siteName = values.GetString(SiteName) ?? throw new ExecutionException("Invalid site name");

            // Get the username and password
            // See https://stackoverflow.com/a/45083787/807064 if we ever want to accept an authorization file
            string username = values.GetString(Username) ?? throw new ExecutionException("Invalid username");
            string password = values.GetString(Password) ?? throw new ExecutionException("Invalid password");
            byte[] authParameterBytes = Encoding.ASCII.GetBytes(username + ":" + password);
            string authParameter = Convert.ToBase64String(authParameterBytes);

            // If a zip file is already provided, use that
            IFile zipFile = null;
            FilePath zipPath = values.GetFilePath(ZipPath);
            if (zipPath != null)
            {
                zipFile = context.FileSystem.GetFile(zipPath);
            }

            // If we don't have a zip file, create one
            if (zipFile == null)
            {
                // If the directory is null, use the output directory
                DirectoryPath directory = values.GetDirectoryPath(Directory, context.FileSystem.GetOutputPath()) ?? context.FileSystem.GetOutputPath();

                // Create the zip file
                zipFile = ZipFileHelper.CreateZipFile(context, directory);
            }

            // Sanity check
            if (!zipFile.Exists)
            {
                throw new ExecutionException($"Zip file at {zipFile.Path} does not exist");
            }

            // Upload it via Kudu REST API
            context.LogDebug($"Starting App Service deployment to {siteName}...");
            using (Stream zipStream = zipFile.OpenRead())
            {
                using (HttpClient client = context.CreateHttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authParameter);
                    System.Net.Http.StreamContent zipContent = new System.Net.Http.StreamContent(zipStream);
                    HttpResponseMessage response = await client.PostAsync($"https://{siteName}.scm.azurewebsites.net/api/zipdeploy", zipContent, context.CancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        context.LogError($"App Service deployment error: {response.StatusCode} {responseContent}");
                        response.EnsureSuccessStatusCode();
                    }
                    else
                    {
                        context.LogDebug($"App Service deployment success to {siteName}");
                    }
                }
            }

            return await input.YieldAsync();
        }
    }
}

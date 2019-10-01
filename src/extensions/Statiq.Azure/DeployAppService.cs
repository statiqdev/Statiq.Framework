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

        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password)
            : this(siteName, username, password, Config.FromValue((DirectoryPath)null))
        {
        }

        // Directory is from root (not input)
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password, Config<DirectoryPath> directory)
            : base(
                new Dictionary<string, IConfig>
                {
                    { nameof(SiteName), siteName ?? throw new ArgumentNullException(nameof(SiteName)) },
                    { nameof(Username), username ?? throw new ArgumentNullException(nameof(Username)) },
                    { nameof(Password), password ?? throw new ArgumentNullException(nameof(Password)) },
                    { nameof(Directory), directory ?? throw new ArgumentNullException(nameof(Directory)) }
                },
                false)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Get the site name
            string siteName = values.GetString(nameof(SiteName)) ?? throw new ExecutionException("Invalid site name");

            // Get the username and password
            // See https://stackoverflow.com/a/45083787/807064 if we ever want to accept an authorization file
            string username = values.GetString(nameof(Username)) ?? throw new ExecutionException("Invalid username");
            string password = values.GetString(nameof(Password)) ?? throw new ExecutionException("Invalid password");
            byte[] authParameterBytes = Encoding.ASCII.GetBytes(username + ":" + password);
            string authParameter = Convert.ToBase64String(authParameterBytes);

            // If the directory is null, use the output directory
            DirectoryPath directory = values.GetDirectoryPath(nameof(Directory), context.FileSystem.GetOutputPath()) ?? context.FileSystem.GetOutputPath();

            // Create the zip file
            IFile zipFile = ZipFileHelper.CreateZipFile(context, directory);

            // Upload it via Kudu REST API
            using (Stream zipStream = zipFile.OpenRead())
            {
                using (HttpClient client = context.CreateHttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authParameter);
                    System.Net.Http.StreamContent zipContent = new System.Net.Http.StreamContent(zipStream);
                    HttpResponseMessage response = await client.PostAsync($"https://{siteName}.scm.azurewebsites.net/api/zipdeploy", zipContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        context.LogError($"App Service deployment error: {response.StatusCode} {responseContent}");
                        response.EnsureSuccessStatusCode();
                    }
                    else
                    {
                        context.LogInformation($"App Service deployment success to {siteName}");
                    }
                }
            }

            return await input.YieldAsync();
        }
    }
}

using System.Reflection;
using GitLabApiClient;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace GLvler
{
    public class GLvler
    {
        public static IConfiguration LoadConfig()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
            var configPath = Path.Combine(projectRoot, "config");
            return new ConfigurationBuilder()
                .SetBasePath(configPath)
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static async Task<Dictionary<string, int>> ShowCounterAsync(string source, string gitlabHostUrl = "")
        {
            var config = LoadConfig();
            if (source.Equals("github", StringComparison.OrdinalIgnoreCase))
                return await UseGithubAsync(config["GithubToken"]);
            else if (source.Equals("gitlab", StringComparison.OrdinalIgnoreCase))
                return await UseGitLabAsync(config["GitLabToken"], string.IsNullOrEmpty(gitlabHostUrl) ? config["GitLabUrl"] : gitlabHostUrl);
            else
                throw new Exception("Unknown source specified.");
        }

        public static async Task<Dictionary<string, int>> UseGithubAsync(string token)
        {
            var client = new GitHubClient(new ProductHeaderValue("CommitCounter"))
            {
                Credentials = new Credentials(token)
            };

            var repos = await client.Repository.GetAllForCurrent();
            var totals = new Dictionary<string, int>();

            foreach (var repo in repos)
            {
                var stats = await client.Repository.Statistics.GetContributors(repo.Owner.Login, repo.Name);
                if (stats == null) continue;

                foreach (var contributor in stats)
                {
                    string login = contributor.Author.Login;
                    int commits = contributor.Total;
                    totals[login] = totals.GetValueOrDefault(login, 0) + commits;
                }
            }
            return totals;
        }

        public static async Task<Dictionary<string, int>> UseGitLabAsync(string token, string gitlabHostUrl)
        {
            if (string.IsNullOrEmpty(token))
                throw new Exception("GitLab token is missing.");

            if (string.IsNullOrEmpty(gitlabHostUrl))
                throw new Exception("GitLab Url is missing.");

            var client = new GitLabClient(gitlabHostUrl, token);

            var projects = await client.Projects.GetAsync();

            var commitTotals = new Dictionary<string, int>();

            foreach (var project in projects)
            {
                var commits = await client.Commits.GetAsync(project.Id);

                foreach (var commit in commits)
                {
                    string authorName = commit.AuthorName;
                    commitTotals[authorName] = commitTotals.GetValueOrDefault(authorName, 0) + 1;
                }
            }

            return commitTotals;
        }
    }
}

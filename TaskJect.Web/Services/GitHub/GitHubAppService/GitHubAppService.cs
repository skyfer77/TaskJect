using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using TaskJect.Web.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Domain.Database;

namespace TaskJect.Web.Services
{
    public class GitHubAppService : IGitHubAppService
    {
        private readonly string _appId;
        private readonly string _privateKey;
        private readonly string _userAgent;
        private readonly string _installation;
        private readonly string _getRepo;
        private readonly string _repos;

        public GitHubAppService(IConfiguration config)
        {
            _appId = config["GitHub:AppId"];
            _privateKey = config["GitHub:PrivateKey"];
            _userAgent = config["GitHub:UserAgent"];
            _installation = config["GitHub:Url:Installation"];
            _getRepo = config["GitHub:Url:GetRepo"];
            _repos = config["GitHub:Url:Repos"];
        }

        // Генеруємо JWT для GitHub App
        private string generateJwtToken()
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(_privateKey);

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );
            var now = DateTimeOffset.UtcNow;

            var iat = now.AddSeconds(-30).ToUnixTimeSeconds();
            var exp = now.AddMinutes(10).ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
				// iat має бути числом у секундах
				new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            };

            var token = new JwtSecurityToken(
                issuer: _appId,
                audience: null, // для GitHub App не потрібен
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Отримуємо installation token для конкретної установки App
        public async Task<GitHubInstallationToken> GetInstallationTokenAsync(long installationId)
        {
            var jwt = generateJwtToken();

            var request = new SendRequest()
            {
                Method = HttpMethod.Post,
                Url = $"{_installation}{installationId}/access_tokens",
                Content = new { },
                Token = jwt
            };

            var response = await sendRequestAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub API error: {response.StatusCode} - {content}");
            }
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubInstallationTokenResponse>(options);

            return new GitHubInstallationToken
            {
                Token = tokenResponse.Token,
                ExpiresAt = tokenResponse.Expires_At
            };
        }

        private class GitHubInstallationTokenResponse
        {
            public string Token { get; set; }
            public DateTime Expires_At { get; set; }
        }

        public async Task<bool> DeleteInstallationGitHubAsync(long installationId)
        {
            var jwt = generateJwtToken();

            var request = new SendRequest()
            {
                Method = HttpMethod.Delete,
                Url = $"{_installation}{installationId}",
                Token = jwt
            };

            var response = await sendRequestAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GitHub API error: {response.StatusCode} - {content}");
                return false;
            }

            return true;
        }

        public async Task<List<GitHubRepoViewModel.RepoItem>> GetRepositoriesAsync(long installationId)
        {
            var token = await GetInstallationTokenAsync(installationId);

            var request = new SendRequest()
            {
                Method = HttpMethod.Get,
                Url = _getRepo,
                Token = token.Token
            };

            var response = await sendRequestAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("repositories")
                .EnumerateArray()
                .Select(r => new GitHubRepoViewModel.RepoItem
                {
                    Id = r.GetProperty("id").GetInt64(),
                    Name = r.GetProperty("name").GetString(),
                    FullName = r.GetProperty("full_name").GetString()
                })
                .ToList();
        }

        public async Task<string> GetDefaultBranchAsync(string owner, string repo, string token)
        {
            var request = new SendRequest()
            {
                Method = HttpMethod.Get,
                Url = $"{_repos}{owner}/{repo}",
                Token = token
            };

            var response = await sendRequestAsync(request);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var repoInfo = await response.Content.ReadFromJsonAsync<GitHubRepoResponse>(options);

            return repoInfo.default_branch;
        }

        private class GitHubRepoResponse
        {
            public string default_branch { get; set; }
        }

        //Створення гілки
        public async Task<bool> CreateBranch(GitHubCreateBranch model)
        {
            var token = await GetInstallationTokenAsync(model.GitHubInfo.InstallationId.Value);

            var owner = model.GitHubInfo.Owner;
            var repoName = model.GitHubInfo.RepoName;

            var baseBranch = await GetDefaultBranchAsync(owner, repoName, token.Token);

            var sendRequest = new SendRequest()
            {
                Method = HttpMethod.Get,
                Url = $"{_repos}{owner}/{repoName}/git/ref/heads/{baseBranch}",
                Token = token.Token,
            };
            // Отримуємо SHA базової гілки
            var responseBaseBrasnch = await sendRequestAsync(sendRequest);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var baseRef = await responseBaseBrasnch.Content.ReadFromJsonAsync<GitRefResponse>(options);

			if (baseRef?.@object?.sha == null)
{
				return false;
			}

			string sha = baseRef.@object.sha;

            // Створюємо нову гілку
            var createRef = new
            {
                @ref = $"refs/heads/{model.NewBranchName}",
                sha
            };

            var request = new SendRequest()
            {
                Method = HttpMethod.Post,
                Url = $"{_repos}{owner}/{repoName}/git/refs",
                Content = createRef,
                Token = token.Token,
            };

            var response = await sendRequestAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return true; // гілку створено
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.UnprocessableEntity &&
                    error.Contains("Reference already exists"))
                {
                    Console.WriteLine($"Гілка {model.NewBranchName} вже існує.");
                    return false;
                }

                Console.WriteLine($"Помилка створення гілки: {error}");
                return false;
            }
        }

        private class GitRefResponse
        {
            public GitObject @object { get; set; }
        }

        private class GitObject
        {
            public string sha { get; set; }
        }

		public async Task<bool> BranchExists(GitHubInfo model)
        {
			var owner = model.Owner;
			var repoName = model.RepoName;
			var branchName = model.BranchName;

			if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repoName) || string.IsNullOrWhiteSpace(model.BranchName))
			{
				return false;
			}

			var token = await GetInstallationTokenAsync(model.InstallationId.Value);

			var request = new SendRequest()
			{
				Method = HttpMethod.Head,
				Url = $"{_repos}{model.Owner}/{model.RepoName}/branches/{model.BranchName}",
				Token = token.Token,
			};

			var response = await sendRequestAsync(request);

			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return false; // гілки не існує або нема доступу до репозиторію
			}

			return response.IsSuccessStatusCode;
		}


		public async Task<int?> CreateIssue(GitHubCreateIssue model)
        {
            if (model == null)
            {
                return null;
            }
            // Створення нового Issue
            if (model.CreateNewIssue)
            {
                var body = sanitizeBody(model.Body); // видаляємо небажані теги

                model.Body = body;
                // GitHub має обмеження 256
                model.Title = model.Title.Length > 256 ? model.Title.Substring(0, 256) : model.Title;

                var issueNumber = await createGitHubIssue(model);

                return issueNumber;
            }

            if (model.GitHubInfo.GitHubIssueNumber.HasValue)
            {
                var exists = await CheckIssueExists(model.GitHubInfo);

                if (!exists)
                {
                    return null;
                }

                return model.GitHubInfo.GitHubIssueNumber;
            }

            return null;
        }

        // GitHub має обмеження ~65000 символів
        // І має обмеження в тегах
        private string sanitizeBody(string? body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return "";
            }

            // Видаляємо <img> і <video> з усього вмісту
            body = Regex.Replace(body, @"<img\b[^>]*>", "", RegexOptions.IgnoreCase);
            body = Regex.Replace(body, @"<video\b[^>]*>.*?</video>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Видаляємо всі теги, крім дозволених
            string allowedTagsPattern = @"<(\/?(p|b|i|strong|em|ul|li|ol|a|span)[^>]*)>";
            body = Regex.Replace(body, @"<[^>]+>", m =>
            {
                return Regex.IsMatch(m.Value, allowedTagsPattern, RegexOptions.IgnoreCase) ? m.Value : "";
            });

            if (body.Length > 65000)
            {
                body = body.Substring(0, 65000);
            }

            return body;
        }

        public async Task<bool> CheckIssueExists(GitHubInfo model)
        {
            var owner = model.Owner;
            var repoName = model.RepoName;
            var issueNumber = model.GitHubIssueNumber;

            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repoName) || !issueNumber.HasValue)
            {
                return false;
            }

            var token = await GetInstallationTokenAsync(model.InstallationId.Value);

            var request = new SendRequest()
            {
                Method = HttpMethod.Get,
                Url = $"{_repos}{owner}/{repoName}/issues/{issueNumber.Value}",
                Token = token.Token,
            };

            var response = await sendRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        private async Task<int?> createGitHubIssue(GitHubCreateIssue model)
        {
            if (model == null)
            {
                return null;
            }

            var owner = model.GitHubInfo.Owner;
            var repoName = model.GitHubInfo.RepoName;
            var installationId = model.GitHubInfo.InstallationId.Value;

            var converter = new ReverseMarkdown.Converter();
            string markdown = converter.Convert(model.Body);

            var token = await GetInstallationTokenAsync(installationId);

            var payload = new
            {
                title = model.Title,
                body = markdown
            };

            var request = new SendRequest()
            {
                Method = HttpMethod.Post,
                Url = $"{_repos}{owner}/{repoName}/issues",
                Token = token.Token,
                Content = payload
            };

            var response = await sendRequestAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<JObject>(responseString);

            return json?["number"]?.Value<int>();
        }

        public async Task<bool> UpdateIssueState(GitHubInfo model, string issueState)
        {
            var owner = model.Owner;
            var repoName = model.RepoName;
            var issueNumber = model.GitHubIssueNumber.Value;
            var installationId = model.InstallationId.Value;

            var token = await GetInstallationTokenAsync(installationId);

            var payload = new
            {
                state = issueState // "open" або "closed"
            };

            var request = new SendRequest()
            {
                Method = HttpMethod.Patch,
                Url = $"{_repos}{owner}/{repoName}/issues/{issueNumber}",
                Token = token.Token,
                Content = payload
            };

            var response = await sendRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckRepoAccess(long installationId, string? owner, string? repoName)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repoName))
            {
                return false;
            }

            var token = await GetInstallationTokenAsync(installationId);

            var request = new SendRequest()
            {
                Method = HttpMethod.Get,
                Url = $"{_repos}{owner}/{repoName}",
                Token = token.Token
            };

            var response = await sendRequestAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

        private async Task<HttpResponseMessage> sendRequestAsync(SendRequest sendRequest)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            // Встановлюємо User-Agent для GitHub API (обов'язково GitHub використовує його для логів і моніторингу API)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var request = new HttpRequestMessage(sendRequest.Method, sendRequest.Url);

            // Авторизація
            if (!string.IsNullOrEmpty(sendRequest.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sendRequest.Token);
            }

            // Якщо є контент додаємо як JSON
            if (sendRequest.Content != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(sendRequest.Content);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await client.SendAsync(request);
        }

        private class SendRequest
        {
            public HttpMethod Method { get; set; }
            public string Url { get; set; }
            public object Content { get; set; } = null;
            public string Token { get; set; } = null;
        }
    }
}
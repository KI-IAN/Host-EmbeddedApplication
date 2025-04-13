using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.Interfaces;
using SharedComponents.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace HostApplication.Controllers
{
    public class EmbeddedApplicationController : Controller
    {

        private readonly EmbeddedAppConfiguration _embeddedAppConfiguration;
        private readonly JwtSettings _jwtSettings;
        private readonly HttpClient _httpClient;
        private readonly IHtmlParser _htmlParser;


        public EmbeddedApplicationController(
            IOptions<EmbeddedAppConfiguration> embeddedAppConfig,
            IOptions<JwtSettings> jwatSettings,
            HttpClient httpClient,
            IHtmlParser htmlParser)
        {
            _embeddedAppConfiguration = embeddedAppConfig.Value;
            _jwtSettings = jwatSettings.Value;
            _httpClient = httpClient;
            _htmlParser = htmlParser;
        }

        public IActionResult Index(string updateRelativeToAbsolutePath)
        {
            try
            {
                var embeddedAppSettings = new EmbeddedAppConfiguration
                {
                    EmbeddedAppBaseURL = _embeddedAppConfiguration.EmbeddedAppBaseURL,
                    ProxyURL = $"{Url.Content($"{_embeddedAppConfiguration.ProxyURL}")}?updateRelativeToAbsolutePath={Uri.EscapeDataString(updateRelativeToAbsolutePath)}",
                    Routes = _embeddedAppConfiguration.Routes,
                };

                return View(embeddedAppSettings);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ActionResult> ProxyPage(string updateRelativeToAbsolutePath)
        {
            try
            {
                Guid userID = Guid.NewGuid();
                var jwt = GenerateJwtToken();

                var uriBuilder = new UriBuilder(_embeddedAppConfiguration.EmbeddedAppBaseURL)
                {
                    Path = _embeddedAppConfiguration.Routes.FirstOrDefault(r => r.API == "SingleSignOn")?.URL ?? string.Empty,
                    Query = $"updateRelativeToAbsolutePath={Uri.EscapeDataString(updateRelativeToAbsolutePath)}"
                };

                AddJWTTokenInHeader(jwt);

                string content = await GetWebPageContentOfEmbeddedApp(uriBuilder);

                content = await UpdatePathInHostingAppIfApplicable(updateRelativeToAbsolutePath, content);

                return Content(content, "text/html");
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(500, $"HTTP error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [NonAction]
        private async Task<string> UpdatePathInHostingAppIfApplicable(string updateRelativeToAbsolutePath, string content)
        {
            if (updateRelativeToAbsolutePath.Equals(SharedComponents.Constants.AppConstants.UpdateRelativeToAbsolutePathInHostingApp))
            {
                content = await _htmlParser.ConvertRelativeToAbsoluteURL(html: content, baseUrl: _embeddedAppConfiguration.EmbeddedAppBaseURL);
            }

            return content;
        }

        [NonAction]
        private async Task<string> GetWebPageContentOfEmbeddedApp(UriBuilder uriBuilder)
        {
            var response = await _httpClient.GetAsync(uriBuilder.Uri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        [NonAction]
        private void AddJWTTokenInHeader(string jwt)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        [NonAction]
        private string GenerateJwtToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = "YourIssuer",
                Audience = "YourAudience",
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials,
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, "john_doe"),
                new Claim(ClaimTypes.Email, "john.doe@example.com"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), // user ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token ID
                })
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }

    }
}

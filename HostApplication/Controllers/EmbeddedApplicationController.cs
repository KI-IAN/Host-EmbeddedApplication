using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
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


        public EmbeddedApplicationController(IOptions<EmbeddedAppConfiguration> embeddedAppConfig, IOptions<JwtSettings> jwatSettings)
        {
            _embeddedAppConfiguration = embeddedAppConfig.Value;
            _jwtSettings = jwatSettings.Value;
        }

        public IActionResult Index()
        {
            try
            {

                _embeddedAppConfiguration.ProxyURL = Url.Content($"{_embeddedAppConfiguration.ProxyURL}");

                return View(_embeddedAppConfiguration);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ActionResult> ProxyPage()
        {
            try
            {
                Guid userID = Guid.NewGuid();
                var jwt = GenerateJwtToken();

                var uriBuilder = new UriBuilder(_embeddedAppConfiguration.EmbeddedAppBaseURL)
                {
                    Path = _embeddedAppConfiguration.Routes.FirstOrDefault(r => r.API == "SingleSignOn")?.URL ?? string.Empty,
                    Query = $"userID={Uri.EscapeDataString(userID.ToString())}"
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                var response = await httpClient.GetAsync(uriBuilder.Uri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
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
        private string GenerateJwtToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = "YourIssuer",
                Audience = "YourAudience",
                Expires = DateTime.UtcNow.AddMinutes(30), // No need to manually set "exp" claim
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

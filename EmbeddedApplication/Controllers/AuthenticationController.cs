using Microsoft.AspNetCore.Mvc;
using SharedComponents.Models;
using System.Security.Claims;

namespace EmbeddedApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult SingleSignOn(string updateRelativeToAbsolutePath)
        {
            try
            {
                // Extract JWT Token from Authorization Header
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

                // Get User Claims
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var userClaims = claimsIdentity?.Claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value }).ToList();

                // Create strongly typed model
                var model = new SingleSignOnModel
                {
                    JwtToken = jwtToken,
                    UserClaims = userClaims ?? [] // Avoid null issues
                };

                ViewData["updateRelativeToAbsolutePath"] = updateRelativeToAbsolutePath;

                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the JWT token and claims: {ex.Message}");
            }
        }

    }
}

namespace SharedComponents.Models
{
    public class SingleSignOnModel
    {
        public string JwtToken { get; set; }
        public List<UserClaim> UserClaims { get; set; }
    }
}
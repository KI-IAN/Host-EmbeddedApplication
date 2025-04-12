namespace SharedComponents.Models
{
    public class EmbeddedAppConfiguration
    {
        public string ProxyURL { get; set; }
        public string EmbeddedAppBaseURL { get; set; }
        public List<Route> Routes { get; set; }
    }
}

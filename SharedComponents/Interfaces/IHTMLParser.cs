namespace SharedComponents.Interfaces
{
    public interface IHtmlParser
    {
        Task<string> ConvertRelativeToAbsoluteURL(string html, string baseUrl);
    }
}

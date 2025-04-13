using AngleSharp;
using SharedComponents.Interfaces;

namespace SharedComponents.Services
{
    public class HtmlParser : IHtmlParser
    {
        public async Task<string> ConvertRelativeToAbsoluteURL(string html, string baseUrl)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            foreach (var element in document.QuerySelectorAll("link[href], script[src], a[href]"))
            {
                var attribute = element.TagName.Equals("script", StringComparison.OrdinalIgnoreCase) ? "src" : "href";
                var value = element.GetAttribute(attribute);

                if (!string.IsNullOrEmpty(value) && !Uri.IsWellFormedUriString(value, UriKind.Absolute))
                {
                    element.SetAttribute(attribute, new Uri(new Uri(baseUrl), value).ToString());
                }
            }

            return document.DocumentElement.OuterHtml;
        }
    }

}

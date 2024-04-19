using Bankkeeper.Structures;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Bankkeeper.Structures.Parsers
{
    public partial class FoodParser
    {
        public ITransaction Parse(string body)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var cost = ParseTotal(doc);
            var time = ParseTime(doc);
            var id = ParseId(doc);

            var res = new Food
            {
                Description = $"[{id}] beFood",
                Timestamp = time,
                Cost = cost,
                Notes = $"[Automated transaction]\n{ParseDescription(doc)}"
            };

            return res;
        }
    }
}
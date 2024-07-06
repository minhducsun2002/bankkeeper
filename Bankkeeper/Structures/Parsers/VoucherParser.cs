using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Bankkeeper.Structures.Transactions;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Bankkeeper.Structures.Parsers
{
    public class VoucherParser
    {
        private static readonly Regex KeyRegex = new Regex("<b>(.*)<\\/b>:", RegexOptions.Compiled);
        
        public ITransaction Parse(string body)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var element = doc.QuerySelector("p")!;
            var text = element.InnerHtml;
            var slice = text.Split("<br>").Select(s => s.Trim()).Where(r => r.StartsWith("<b>"));
            var keys = slice.Select(r =>
            {
                var match = KeyRegex.Match(r);
                Debug.Assert(match.Success);
                var key = match.Groups[1].Value;
                var startValue = match.Groups[0].Index + match.Groups[0].Length;
                return (key, r[startValue..].Replace("<br>", ""));
            }).ToDictionary();

            var price = int.Parse(string.Join("", keys["Price"].Where(char.IsDigit)));
            var time = DateTimeOffset.Parse(keys["Date purchase"], CultureInfo.InvariantCulture);
            var finalTime = new DateTimeOffset(
                time.Year,
                time.Day,
                time.Month,
                time.Hour,
                time.Minute,
                time.Second,
                TimeSpan.FromHours(7)
            );
            
            var res = new Voucher
            {
                Description = "be voucher",
                Cost = price,
                Timestamp = finalTime,
                Notes = keys.GetValueOrDefault("Subscription", "")
            };

            return res;
        }
    }
}
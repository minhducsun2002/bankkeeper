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
            var time = ParseTime(keys["Date purchase"].Trim());
            
            var res = new Voucher
            {
                Description = "be voucher",
                Cost = price,
                Timestamp = time,
                Notes = keys.GetValueOrDefault("Subscription", "")
            };

            return res;
        }
        
        private DateTimeOffset ParseTime(string s)
        {
            s = s.Replace(".", "");
            var r = s.Split(' ');
            var time = r[1].Trim().Split(':');
            var date = r[0].Trim().Split('-');

            var final = new DateTimeOffset(
                int.Parse(date[2]),
                int.Parse(date[1]),
                int.Parse(date[0]),
                int.Parse(time[0]),
                int.Parse(time[1]),
                0,
                TimeSpan.FromHours(7)
            );

            return final;
        }
    }
}
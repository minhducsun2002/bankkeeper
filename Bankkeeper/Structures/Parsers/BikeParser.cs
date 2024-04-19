using System.Text;
using Bankkeeper.Structures.Transactions;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Bankkeeper.Structures.Parsers
{
    public class BikeParser
    {
        public ITransaction Parse(string body)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var rows = doc.QuerySelectorAll("div[class=\"w-100\"]");
            var rowContents = rows.Select(r => r.GetChildElements().Last().InnerText.Trim()).ToArray();

            string a1 = rowContents[0], a2 = rowContents[2];
            DateTimeOffset start = ParseTime(rowContents[1]), end = ParseTime(rowContents[3]);

            var t = doc.QuerySelectorAll("table[cellpadding=\"0\"]")[12];
            var totalRow = t.GetChildElements().Last();
            var total = totalRow.GetChildElements().Last().InnerText;
            var cost = string.Join("", total.Where(char.IsDigit));

            
            var desc = $"Trip to {a2}";
            var home = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrWhiteSpace(home) && a2.Contains(home))
            {
                desc = "đi xe về nhà";
            }
            
            var res = new Bike
            {
                Description = desc,
                Cost = int.Parse(cost),
                Timestamp = end,
                Notes = ParseDescription(a1, a2, start, end) 
            };

            return res;
        }

        private DateTimeOffset ParseTime(string s)
        {
            var r = s.Split(',');
            var time = r[0].Trim().Split(':');
            var date = r[1].Trim().Split('/');

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

        private string ParseDescription(string start, string end, DateTimeOffset s, DateTimeOffset e)
        {
            var st = new StringBuilder();
            st.AppendLine($"[{s:HH:mm}]");
            st.AppendLine($"{start}");
            st.AppendLine();
            st.AppendLine($"[{e:HH:mm}]");
            st.AppendLine($"{end}");
            
            return st.ToString();
        }
    }
}
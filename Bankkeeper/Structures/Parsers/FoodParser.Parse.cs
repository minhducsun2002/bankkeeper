using System.Text;
using System.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Bankkeeper.Structures.Parsers
{
    public partial class FoodParser
    {
        private int ParseTotal(HtmlDocument doc)
        {
            const string tableStyle = "table[cellpadding=\"0\"][cellspacing=\"0\"][width=\"100%\"][border=\"0\"]";
            try
            {
                var costTable = doc.QuerySelectorAll(tableStyle)[14];
                var totalCell = costTable!.QuerySelectorAll("tr").Last().GetChildElements().Last();
                var text = string.Concat(totalCell.InnerText.Trim().Where(char.IsDigit));

                return int.Parse(text);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 0;
            }
        }

        private string ParseId(HtmlDocument doc)
        {
            const string dateSelector = "td[align=\"right\"]";
            var line = doc.QuerySelectorAll(dateSelector)[1];
            var text = line.InnerText.Trim().Split(':').Last().Trim();
            return text;
        }

        private DateTimeOffset ParseTime(HtmlDocument doc)
        {
            const string dateSelector = "td[align=\"right\"]";
            
            var date = doc.QuerySelector(dateSelector);
            var datePiece = date.InnerText.Split(',').Last().Trim().Split('/');
            int month = int.Parse(datePiece[0]), day = int.Parse(datePiece[1]), year = int.Parse(datePiece[2]);

            var time = doc.QuerySelectorAll("table")[13];
            var row = time.GetChildElements();
            var cell = row.First().GetChildElements().Skip(1).First();
            var line = cell.GetChildElements().First();
            var linePiece = line.InnerText.Trim().Split(':');
            int hour = int.Parse(linePiece[0]), minute = int.Parse(linePiece[1]);

            // UTC+7
            var d = new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.FromHours(7));
            return d;
        }

        private string ParseDescription(HtmlDocument doc)
        {
            var b = new StringBuilder();
            b.AppendLine($"Đơn hàng beFood {ParseId(doc)}");
            b.AppendLine();

            var stations = ParseStations(doc);
            foreach (var (ts, name, address) in stations)
            {
                b.AppendLine($"[{ts}] {name}");
                b.AppendLine(address);
            }

            return b.ToString();
        }

        private (string, string, string)[] ParseStations(HtmlDocument doc)
        {
            var rows = doc.QuerySelectorAll("table")[13].GetChildElements();

            var res = rows.Select(row =>
            {
                var elements = row.GetChildElements().Skip(1).First().GetChildElements().ToArray();

                var time = elements[0].InnerText;
                var name = elements[1];
                var address = elements[3];

                var rr = (
                    time.Trim(),
                    HttpUtility.HtmlDecode(name.InnerText.Trim()),
                    HttpUtility.HtmlDecode(address.InnerText.Trim())
                );

                return rr;
            });

            return res.ToArray();
        }
    }
}
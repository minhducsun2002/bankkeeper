using System.Text;
using System.Web;
using Bankkeeper.Structures.Transactions;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Bankkeeper.Structures.Parsers
{
    public class DeliveryParser
    {
        public ITransaction Parse(string body)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var steps = doc.QuerySelectorAll("tr[valign=\"top\"]");
            var info = steps.Select(st =>
            {
                var child = st.GetChildElements();
                var c = child.Last().InnerText.Trim();
                var pieces = c.Split(':');
                var time = (hour: int.Parse(pieces[0]), minute: int.Parse(pieces[1]));

                var meta = st.NextSiblingElement();
                var metaNode = meta.GetChildElements().Last();
                var person = metaNode.GetChildElements().First().InnerText.Trim();
                var address = metaNode.ChildNodes.Last().InnerText.Trim();

                return (time, person, address);
            }).ToList();

            var meta = doc.QuerySelectorAll(
                "table[cellpadding=\"0\"][cellspacing=\"0\"][width=\"100%\"][role=\"presentation\"] td[align=\"right\"]"
            );

            var dateText = meta[0].InnerText.Trim();
            var date = ParseDate(dateText);
            var bookingId = meta[1].InnerText.Trim().Split(":").Last().Trim();
            var times = info.Select(r =>
            {
                var t = r.time;
                var dto = ParseTime(date, t);
                return dto;
            }).ToList();
            

            var costNode = doc.QuerySelectorAll("table[cellpadding=\"5\"] td[align=\"right\"]").Last();
            var costText = HttpUtility.HtmlDecode(costNode.InnerText.Trim());
            var cost = int.Parse(string.Join("", costText.Where(char.IsDigit)));

            if (cost % 1000 != 0)
            {
                throw new Exception("Parsing failed: potentially wrong cost " + cost);
            }
            
            var res = new Delivery
            {
                Description = $"Delivery {bookingId} to {info[1].address}",
                Cost = cost,
                Timestamp = times[0],
                Notes = ParseDescription(
                    (info[0].person, info[0].address), 
                    (info[1].person, info[1].address), 
                    times[0], times[1]
                )
            };

            return res;
        }

        private DateTimeOffset ParseTime((int year, int month, int day) date, (int hour, int minute) time)
        {
            var final = new DateTimeOffset(
                date.year,
                date.month,
                date.day,
                time.hour,
                time.minute,
                0,
                TimeSpan.FromHours(7)
            );

            return final;
        }

        private (int year, int month, int day) ParseDate(string s)
        {
            var l = s.Split("/").Select(int.Parse).ToList();
            return (l[2], l[1], l[0]);
        }
        
        private string ParseDescription(
            (string sender, string address) start, (string sender, string address) end, DateTimeOffset s, DateTimeOffset e
        )
        {
            var st = new StringBuilder();
            st.AppendLine($"[{s:HH:mm}]");
            st.AppendLine($"{start.sender} - {start.address}");
            st.AppendLine();
            st.AppendLine($"[{e:HH:mm}]");
            st.AppendLine($"{end.sender} - {end.address}");
            
            return st.ToString();
        }
    }
}
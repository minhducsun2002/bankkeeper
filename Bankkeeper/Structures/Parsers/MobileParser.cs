using Bankkeeper.Structures.Transactions;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using MimeKit;

namespace Bankkeeper.Structures.Parsers
{
    public class MobileParser
    {
        public ITransaction Parse(MimeMessage message)
        {
            // we need an attachment
            var attachment = message.Attachments.Where(
                a => a is TextPart p 
                     && p.ContentDisposition?.FileName.ToLowerInvariant().EndsWith(".html") == true
                     && p.ContentType.MimeType == "text/html"
            );
            var receipt = attachment.FirstOrDefault();
            if (receipt == null)
            {
                throw new Exception("Mobile bill has no attachment!");
            }

            var a = (TextPart)receipt;
            var body = a.Text;
            
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var tables = doc.QuerySelectorAll("table").ToList();
            var table = tables.Last();
            var lastRow = table.QuerySelectorAll("tr")!.Last();
            var lastCell = lastRow.QuerySelectorAll("td")!.Last();
            var costText = string.Join("", lastCell.InnerText.Where(char.IsDigit));
            var cost = int.Parse(costText);

            var meta = tables[13];
            var cells = meta.QuerySelectorAll("td");
            var code = cells[1].InnerText.Trim();
            if (code.StartsWith(':'))
            {
                code = code[1..].Trim();
            }
            var timeText = cells[3].InnerText.Trim();
            if (timeText.StartsWith(':'))
            {
                timeText = timeText[1..].Trim();
            }
            var time = ParseTime(timeText);
            
            var res = new MobileTopup
            {
                Description = "Recharge ID " + code,
                Cost = cost,
                Timestamp = time,
                Notes = ""
            };

            return res;
        }
        
        private DateTimeOffset ParseTime(string s)
        {
            s = s.Replace(".", "");
            var r = s.Split(',');
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
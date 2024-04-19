using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bankkeeper.Structures;
using Bankkeeper.Structures.Firefly.Requests;
using Bankkeeper.Structures.Firefly.Responses;
using Bankkeeper.Structures.Parsers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

dotenv.net.DotEnv.Load();

var token = Environment.GetEnvironmentVariable("TOKEN");

var http = new HttpClient();
var client = new ImapClient();

client.Connect("mail.messenger.nope.ovh");
await client.AuthenticateAsync(Environment.GetEnvironmentVariable("USER"), Environment.GetEnvironmentVariable("PASS"));

var inbox = client.Inbox;
await inbox.OpenAsync(FolderAccess.ReadWrite);
Console.WriteLine ("Total messages: {0}", inbox.Count);
Console.WriteLine ("Unread messages: {0}", inbox.Unread);

var food = "no-reply@be.xyz";
var bike = "no-reply@be.com.vn";

var unseenIds = inbox.Search(SearchQuery.NotSeen).OrderBy(r => r.Id);
foreach (var id in unseenIds)
{
    var message = inbox.GetMessage(id);
    var sender = ((MailboxAddress)message.From[0]).Address;
    var subject = message.Subject;
    var body = message.HtmlBody;

    if (sender == food)
    {
        try
        {
            var parser = new FoodParser();
            var transaction = parser.Parse(body);
            await Handle(transaction);
            await inbox.AddFlagsAsync(id, MessageFlags.Seen, false);
            Console.WriteLine("Processed {0}, marked as read.", subject);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error processing {0}: {1}", subject, e);
        }
    }

    if (sender == bike)
    {
        try
        {
            var parser = new BikeParser();
            var transaction = parser.Parse(body);
            await Handle(transaction);
            await inbox.AddFlagsAsync(id, MessageFlags.Seen, false);
            Console.WriteLine("Processed {0}, marked as read.", subject);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error processing {0}: {1}", subject, e);
        }
    }
}

async Task Handle(ITransaction transaction)
{
    var t = transaction.SerializeIntoTransaction();

    var json = JsonSerializer.Serialize(t, new JsonSerializerOptions
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    var req = new HttpRequestMessage
    {
        RequestUri = new Uri("https://m.cipher.moe/api/v1/transactions"),
        Method = HttpMethod.Post,
        Headers =
        {
            { "Authorization", "Bearer " + token },
            { "Accept", "application/json" }
        },
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    var a = await http.SendAsync(req);
    var response = await a.Content.ReadFromJsonAsync<CreateTransactionResponse>();

    if (response!.Data?.Id != null)
    {
        Console.WriteLine("Created transaction ID {0}, description \"{1}\"", response.Data.Id, transaction.Description);
    }
    else
    {
        throw new Exception($"Error posting {transaction.Description}: {response.Message}");
    }
}

await client.DisconnectAsync(true);
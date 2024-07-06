using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bankkeeper.Structures;
using Bankkeeper.Structures.Firefly;
using Bankkeeper.Structures.Parsers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

string[] advertisementsPrefix = ["[AD]", "[QC]"];

dotenv.net.DotEnv.Load();

var token = Environment.GetEnvironmentVariable("TOKEN");
var user = Environment.GetEnvironmentVariable("USER");
var pass = Environment.GetEnvironmentVariable("PASS");
var smtp = Environment.GetEnvironmentVariable("SMTP");

ArgumentException.ThrowIfNullOrWhiteSpace(token);
ArgumentException.ThrowIfNullOrWhiteSpace(user);
ArgumentException.ThrowIfNullOrWhiteSpace(pass);
ArgumentException.ThrowIfNullOrWhiteSpace(smtp);

var client = new ImapClient();

Console.Error.WriteLine("Beginning loop...");

while (true)
{
    try
    {
        await Work();
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
    }
    Console.Error.WriteLine("Waiting for 5 minutes...");
    await Task.Delay(TimeSpan.FromMinutes(5));
}

async Task Work()
{
    client.Connect(smtp);
    await client.AuthenticateAsync(user, pass);
    Console.Error.WriteLine("=> Logged in!");

    var inbox = client.Inbox;
    await inbox.OpenAsync(FolderAccess.ReadWrite);

    const string food = "no-reply@be.xyz", bike = "no-reply@be.com.vn";

    var unseenIds = inbox.Search(SearchQuery.NotSeen).OrderBy(r => r.Id).ToList();

    if (unseenIds.Count == 0)
    {
        Console.Error.WriteLine("=> No new messages!");
    }
    else
    {
        Console.Error.WriteLine("=> Not seen messages: {0}", unseenIds.Count);
    }

    var messages = new List<(UniqueId, MimeMessage)>();

    foreach (var r in unseenIds)
    {
        var res = await inbox.GetMessageAsync(r);
        messages.Add((r, res));
    }
    Console.Error.WriteLine("=> Downloaded {0} messages", messages.Count);

    foreach (var (id, message) in messages)
    {
        var sender = ((MailboxAddress)message.From[0]).Address;
        var body = message.HtmlBody;
        var subject = message.Subject;

        if (advertisementsPrefix.Any(s => subject.StartsWith(s)))
        {
            // skip advertisements
            await inbox.AddFlagsAsync(id, MessageFlags.Seen, false);
            Console.WriteLine("   Ignoring {0} as advertisements. Marking as read.", message.MessageId);
            continue;
        }

        if (sender is food or bike)
        {
            try
            {
                var transaction = sender == food
                    ? new FoodParser().Parse(body) 
                    : (subject.Contains("ride details") ? new BikeParser().Parse(body) : new VoucherParser().Parse(body));
                var c = new TransactionClient(token);
                await c.Handle(transaction);
                await inbox.AddFlagsAsync(id, MessageFlags.Seen, false);
                Console.WriteLine("=> Processed {0}, marked as read.", message.MessageId);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("=> Error processing {0}: {1}", message.MessageId, e);
            }
        }
    }

    await client.DisconnectAsync(true);
    Console.Error.WriteLine("=> Logged out!");
}
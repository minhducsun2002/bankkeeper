using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bankkeeper.Structures.Firefly.Responses;

namespace Bankkeeper.Structures.Firefly
{
    public class TransactionClient
    {
        private static readonly HttpClient HttpClient = new();
        private readonly string token;
        public TransactionClient(string token)
        {
            this.token = token;
        }

        public async Task Handle(ITransaction transaction)
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

            var a = await HttpClient.SendAsync(req);
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
    }
}
using System.Text.Json.Serialization;

namespace Bankkeeper.Structures.Firefly.Requests
{
    public class TransactionSplit
    {
        public string Type { get; set; } = "withdrawal";
        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
        public int Amount { get; set; }
        public string Description { get; set; } = "Automated transaction";
        public string Notes { get; set; } = "";

        [JsonPropertyName("source_name")]
        public string SourceName { get; set; } = "Source";
        
        [JsonPropertyName("destination_name")]
        public string DestinationName { get; set; } = "Destination";

        public string[] Tags { get; set; } = ["Automated"];
    }
}
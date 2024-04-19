using System.Text.Json.Serialization;

namespace Bankkeeper.Structures.Firefly.Requests
{
    public class Transaction
    {
        [JsonPropertyName("apply_rules")]
        public bool ApplyRules { get; set; } = true;
        
        [JsonPropertyName("transactions")]
        public TransactionSplit[] Splits { get; set; } = Array.Empty<TransactionSplit>();
    }
}
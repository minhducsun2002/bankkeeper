using System.Text.Json.Serialization;

namespace Bankkeeper.Structures.Firefly.Requests
{
    public class Attachment
    {
        [JsonPropertyName("filename")]
        public string FileName { get; set; } = "";
        
        [JsonPropertyName("attachable_type")]
        public string AttachType { get; set; } = "TransactionJournal";
        
        [JsonPropertyName("attachable_id")]
        public string AttachId { get; set; } = "TransactionJournal";
    }
}
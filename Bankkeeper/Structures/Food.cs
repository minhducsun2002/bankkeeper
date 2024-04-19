namespace Bankkeeper.Structures
{
    public class Food : ITransaction
    {
        public string Notes { get; init; } = "";
        public int Cost { get; init; }
        public string Description { get; init; } = "";
        public DateTimeOffset Timestamp { get; init; }
        public byte[] Attachment { get; } = Array.Empty<byte>();
    }
}
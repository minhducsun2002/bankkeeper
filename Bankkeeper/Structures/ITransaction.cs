using Bankkeeper.Structures.Firefly.Requests;

namespace Bankkeeper.Structures
{
    public interface ITransaction
    {
        public string Notes { get; }
        public int Cost { get; }
        public string Description { get; }
        public DateTimeOffset Timestamp { get; }

        public Transaction SerializeIntoTransaction();
    }
}
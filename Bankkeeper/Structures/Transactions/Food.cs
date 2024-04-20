using Bankkeeper.Structures.Firefly.Requests;

namespace Bankkeeper.Structures.Transactions
{
    public class Food : ITransaction
    {
        public string Notes { get; init; } = "";
        public int Cost { get; init; }
        public string Description { get; init; } = "";
        public DateTimeOffset Timestamp { get; init; }
        public Transaction SerializeIntoTransaction()
        {
            var src = Environment.GetEnvironmentVariable("FOOD_SRC_ACCOUNT");
            var dst = Environment.GetEnvironmentVariable("FOOD_DST_ACCOUNT");
            ArgumentException.ThrowIfNullOrWhiteSpace(src);
            ArgumentException.ThrowIfNullOrWhiteSpace(dst);
            
            var t = new Transaction
            {
                Splits =
                [
                    new TransactionSplit
                    {
                        Type = "withdrawal",
                        Date = Timestamp,
                        Amount = Cost,
                        Description = Description,
                        Notes = Notes,
                        SourceName = src,
                        DestinationName = dst
                    }
                ]
            };

            return t;
        }
    }
}
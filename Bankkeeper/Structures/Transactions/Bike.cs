using Bankkeeper.Structures.Firefly.Requests;

namespace Bankkeeper.Structures.Transactions
{
    public class Bike : ITransaction
    {
        public string Notes { get; init; } = "";
        public int Cost { get; init; }
        public string Description { get; init; } = "";
        public DateTimeOffset Timestamp { get; init; }
        public Transaction SerializeIntoTransaction()
        {
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
                        SourceName = "Be-Cake credit card",
                        DestinationName = "Transportation"
                    }
                ]
            };

            return t;
        }
    }
}
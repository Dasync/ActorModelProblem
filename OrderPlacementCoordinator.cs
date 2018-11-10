using System;

public class PurchaseItemCommand : Message
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
}

public class CreditCommand : Message
{
    public string ExternalTransationId { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
}

public class OrderPlacementCoordinator : Actor,
    IReceive<PurchaseItemCommand>
{
    private PersistedValue<Guid> transactionId;

    public void Receive(PurchaseItemCommand command)
    {
        this.transactionId = PersistedValue.Create(Guid.NewGuid());

        var paymentActor = this.System.GetActorRef(
            type: "Payment",
            id: transactionId.Value);

        paymentActor.Send(new CreditCommand
        {
            ExternalTransationId =
                transactionId.Value.ToString(),

            // Hard-coded for simplicity's sake.
            Amount = 99.95,
            Description = "Coffee Beans 1lb"
        });
    }
}

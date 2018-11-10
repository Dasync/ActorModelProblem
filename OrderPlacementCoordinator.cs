using System;

public class PurchaseItemCommand : Message
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
}

public class OrderPlacementCoordinator : Actor,
    IReceive<PurchaseItemCommand>
{
    private PersistedValue<Guid> transactionId;

    public void Receive(PurchaseItemCommand command)
    {
        this.transactionId = PersistedValue.Create(Guid.NewGuid());
    }
}

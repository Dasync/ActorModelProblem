using System;

public class PurchaseItemCommand : Message
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
}

public class PurchaseItemResponse : Message
{
    public bool Succeeded { get; set; }
    public string ErrorCode { get; set; }
}

public class CreditCommand : Message
{
    public string ExternalTransationId { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
}

public class CreditResponse : Message
{
    public string ExternalTransationId { get; set; }
    public string InternalTransationId { get; set; }
    public bool Succeeded { get; set; }
    public string ErrorCode { get; set; }
}

public class VoidTransactionCommand : Message
{
    public string ExternalTransationId { get; set; }
}

public class VoidTransactionResponse : Message
{
    public bool Succeeded { get; set; }
    public string ErrorCode { get; set; }
}

public class ReserveItemCommand : Message
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
    public bool AllOrNothing { get; set; }
}

public class ReserveItemResponse : Message
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
    public bool Succeeded { get; set; }
    public string ErrorCode { get; set; }
}

public class OrderPlacementCoordinator : Actor,
    IReceive<PurchaseItemCommand>,
    IReceive<CreditResponse>,
    IReceive<ReserveItemResponse>,
    IReceive<VoidTransactionResponse>
{
    private PersistedValue<Guid> transactionId;
    private PersistedValue<ActorRef> requestor;
    private PersistedValue<string> itemId;
    private PersistedValue<int> quantity;
    private PersistedValue<string> reservationError;

    public void Receive(PurchaseItemCommand command)
    {
        this.transactionId = PersistedValue.Create(Guid.NewGuid());
        this.requestor = PersistedValue.Create(command.Sender);
        this.itemId = PersistedValue.Create(command.ItemId);
        this.quantity = PersistedValue.Create(command.Quantity);

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

    public void Receive(CreditResponse response)
    {
        if (response.Succeeded)
        {
            var warehouseActor = this.System.GetActorRef(
                type: "Warehouse",
                id: transactionId.Value);

            warehouseActor.Send(new ReserveItemCommand
            {
                ItemId = this.itemId.Value,
                Quantity = this.quantity.Value,
                AllOrNothing = true
            });
        }
        else
        {
            this.requestor.Value.Send(
                new PurchaseItemResponse
                {
                    Succeeded = false,
                    ErrorCode = response.ErrorCode
                });

            this.Terminate();
        }
    }

    public void Receive(ReserveItemResponse response)
    {
        if (response.Succeeded)
        {
            this.requestor.Value.Send(
                new PurchaseItemResponse
                {
                    Succeeded = true
                });

            this.Terminate();
        }
        else
        {
            this.reservationError =
                PersistedValue.Create(response.ErrorCode);

            var paymentActor = this.System.GetActorRef(
                type: "Payment",
                id: transactionId.Value);

            paymentActor.Send(new VoidTransactionCommand
            {
                ExternalTransationId =
                    transactionId.Value.ToString(),
            });
        }
    }

    public void Receive(VoidTransactionResponse response)
    {
        this.requestor.Value.Send(
            new PurchaseItemResponse
            {
                Succeeded = false,
                ErrorCode = this.reservationError.Value
            });

        this.Terminate();
    }
}

namespace Orders.Domain.Entities
{
    public enum OrderStatus
    {
        Received = 1,
        Preparing = 2,
        ReadyToDeliver = 3,
        OnTheWay = 4,
        Delivered = 5,
        DeliveryFailed = 6,
        Cancelled = 7
    }
}
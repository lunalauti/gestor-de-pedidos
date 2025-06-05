namespace Orders.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Received = 1,
    Processing = 2,
    Ready = 3,
    Delivered = 4,
    Cancelled = 5
}
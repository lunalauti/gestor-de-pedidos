namespace Orders.Domain.Enums;

public enum OrderStatus
{
    RECEIVED = 1,
    READY_TO_SHIP = 2,
    OUT_FOR_DELIVERY = 3,
    DELIVERED = 4,
    DELIVERY_FAILED = 5
}
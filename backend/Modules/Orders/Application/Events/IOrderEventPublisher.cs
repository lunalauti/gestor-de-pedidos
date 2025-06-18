using System.Threading.Tasks;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Events
{
    public interface IOrderEventPublisher
    {
        Task PublishOrderCreatedAsync(Order order);
        Task PublishOrderStatusUpdateAsync(Guid orderId, string orderNumber, OrderStatus status, DateTime updatedAt);
    }
}
namespace Orders.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<List<Order>> GetAllAsync();
        Task<List<Order>> GetByStatusAsync(OrderStatus status);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
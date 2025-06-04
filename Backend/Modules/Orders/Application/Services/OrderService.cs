namespace Orders.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;

        public OrderService(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var order = new Order(
                createOrderDto.OrderNumber,
                createOrderDto.CustomerName,
                createOrderDto.CustomerEmail,
                createOrderDto.Address,
                createOrderDto.Phone
            );

            // TODO: Agregar orderID
            foreach (var itemDto in createOrderDto.Items)
            {
                order.AddItem(itemDto.ProductId, itemDto.Quantity, itemDto.ProductName);
            }

            await _orderRepository.AddAsync(order);
            await _eventPublisher.PublishOrderCreatedAsync(order);

            return MapToDto(order);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            var oldStatus = order.Status;
            order.UpdateStatus(newStatus);
            await _orderRepository.UpdateAsync(order);
            
            await _eventPublisher.PublishOrderStatusChangedAsync(order, oldStatus, newStatus);
            return true;
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            await _orderRepository.DeleteAsync(id);
            await _eventPublisher.PublishOrderDeletedAsync(order);
            return true;
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Address = order.Address,
                Phone = order.Phone,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderStatusId = (int)order.Status,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId.ToString(),
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                }).ToList()
            };
        }
    }
}
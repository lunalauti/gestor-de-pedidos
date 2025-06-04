namespace Orders.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(OrderDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by number: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<List<Order>> GetAllAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                throw;
            }
        }

        public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .Where(o => o.Status == status)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status: {Status}", status);
                throw;
            }
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, OrderStatus? status = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking();

                if (status.HasValue)
                {
                    query = query.Where(o => o.Status == status.Value);
                }

                var totalCount = await query.CountAsync();
                
                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (orders, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged orders");
                throw;
            }
        }

        public async Task<Order> AddAsync(Order order)
        {
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order: {OrderNumber}", order.OrderNumber);
                throw;
            }
        }

        public async Task UpdateAsync(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order: {OrderId}", order.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if order exists: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> OrderNumberExistsAsync(string orderNumber)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if order number exists: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<List<Order>> GetByCustomerEmailAsync(string customerEmail)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .Where(o => o.CustomerEmail == customerEmail)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by customer email: {CustomerEmail}", customerEmail);
                throw;
            }
        }

        public async Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by date range: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }
    }
}
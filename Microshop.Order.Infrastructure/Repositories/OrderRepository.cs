using System.Linq.Expressions;
using Microshop.Order.Application.Interfaces;
using Microshop.Order.Infrastructure.Data;
using Microshop.SharedLibrary.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Microshop.Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext dbContext, ILogger<OrderRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<Domain.Entities.Order> GetByAsync(Expression<Func<Domain.Entities.Order, bool>> predicate)
    {
        try
        {
            _logger.LogDebug($"GetByAsync called with predicate");
            var order = await _dbContext.Orders.Where(predicate).FirstOrDefaultAsync();
            if (order == null)
            {
                _logger.LogError($"GetByAsync returned null");
                return null!;
            }
            _logger.LogDebug($"GetByAsync returned {order}");
            return order;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occured while retrieving order");
            throw new InvalidOperationException("Error occured while retrieving order");
        }
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug($"GetAllAsync called");
            var orders = await _dbContext.Orders.ToListAsync();
            _logger.LogDebug($"GetAllAsync returned {orders.Count}");
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while retrieving orders");
            throw new InvalidOperationException("Error occured while retrieving orders");
        }
    }

    public async Task<Domain.Entities.Order> FindByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug($"FindByIdAsync called");
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null)
            {
                _logger.LogDebug($"FindByIdAsync returned null");
                return null!;
            }
            
            _logger.LogDebug("FindByIdAsync returned");
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while retrieving order");
            throw new InvalidOperationException("Error occured while retrieving order");
        }
    }

    public async Task<Response> CreateAsync(Domain.Entities.Order model)
    {
        try
        {
            _logger.LogDebug($"CreateAsync called");
            var createOrder =  _dbContext.Add(model).Entity;
            await _dbContext.SaveChangesAsync();

            if (createOrder is not null)
            {
                _logger.LogInformation("Order created successfully, {OrderId}}", model.Id);
                return new Response(true, $"{model.Id} was successfully created");
            }
            else
            {
                _logger.LogError("Order creation failed");
                return new Response(false, $"{model.Id} failed to create order");
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Error occured while creating order");
            return new Response(false, "Database error occured while adding new order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while creating order");
            return new Response(false, "Error occured while creating order");
        }
    }

    public async Task<Response> UpdateAsync(Domain.Entities.Order model)
    {
        try
        {
            _logger.LogInformation($"UpdateAsync called");
            _dbContext.Orders.Update(model);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Order updated successfully");
                return new Response(true, "Your update was successfully updated");
            }
            else
            {
                _logger.LogWarning("No changes made when updating order");
                return new  Response(false, "No changes made when updating order");
            }
        }
        catch (DbUpdateConcurrencyException conEx)
        {
            _logger.LogError(conEx, "Error occured while updating order");
            return new Response(false, "Order was updated by another user. Please refresh and try again");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occured while updating order {OrderId}", model.Id);
            return new Response(false, "Database error occured while updating order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while updating order");
            return new Response(false, "Error occured while updating order");
        }
    }

    public async Task<Response> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"DeleteAsync called");
            var order = await FindByIdAsync(id);
            if (order is null)
            {
                _logger.LogWarning($"DeleteAsync returned null");
                return new Response(false, "Order was not found");
            }
            _dbContext.Orders.Remove(order);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Order deleted successfully");
                return new Response(true,  "Your order was deleted successfully");
            }
            else
            {
                _logger.LogError("Order deletion failed");
                return new Response(false, "Order was not deleted");
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Error occured while deleting order");
            return new Response(false, "Database error occured while canceling the order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while canceling the order: {orderId}", id);
            return new Response(false, "Error occured while delecting order");
        }
    }
}
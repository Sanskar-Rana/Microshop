using System.Linq.Expressions;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Infrastructure.Data;
using Microshop.SharedLibrary.Logs;
using Microshop.SharedLibrary.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProductRepository> _logger;
    public ProductRepository(AppDbContext dbContext,  ILogger<ProductRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<Product> GetByAsync(Expression<Func<Product, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("Getting product by predicate");
            var product = await _dbContext.Products.Where(predicate).Include(c => c.Category).FirstOrDefaultAsync();
            if (product == null)
            {
                _logger.LogDebug("Product not found with the specified predicate");
                return null!;
            }

            _logger.LogDebug("Product retrieved successfully: {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product by predicate");
            throw new InvalidOperationException("Error occured while detecting the product");
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all products");
            var products = await _dbContext.Products.Include(c => c.Category).ToListAsync();
            _logger.LogDebug("Retrieved {ProductCount} products", products.Count);
            return products ?? new List<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all products");
            throw new InvalidOperationException("Error occured while detecting the product");
        }
    }

    public async Task<Product> FindByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Finding product by ID: {ProductId}", id);
            var product = await _dbContext.Products
                .Include(c => c.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                _logger.LogDebug("Product not found: {ProductId}", id);
                return null!;
            }

            _logger.LogDebug("Product found: {ProductId}", id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while finding product: {ProductId}", id);
            throw new InvalidOperationException("Error occured while detecting the product");
            
        }
    }

    public async Task<Response> CreateAsync(Product model)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", model.Name);
            var getProduct = await GetByAsync(_ => _.Name!.Equals( model.Name));
            if (getProduct is not null && !string.IsNullOrWhiteSpace(getProduct.Name))
            {
                _logger.LogWarning("Product already exists: {ProductName}", model.Name);
                return new Response(false, "Product already exists");
            }
            
            var currentEntity = _dbContext.Products.Add(model);
            await _dbContext.SaveChangesAsync();

            if (currentEntity != null)
            {
                _logger.LogInformation("Product created successfully: {ProductName} with ID: {ProductId}", 
                    model.Name, model.Id);
                return new Response(true, $"{model.Name} added to database successfully");
            }
            else
            {
                _logger.LogWarning("Product creation failed - no entity returned: {ProductName}", model.Name);
                return new Response(false, $"{model.Name} could not be added");
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error creating product: {ProductName}", model.Name);
            return new Response(false, "Database error occurred while adding new product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating product: {ProductName}", model.Name);
            return new Response(false, $"{model.Name} could not be added");
        }
    }

    public async Task<Response> UpdateAsync(Product model)
    {
        try
        {
            _logger.LogInformation("Updating product: {ProductName} with ID: {ProductId}", model.Name, model.Id);
            // var product = await FindByIdAsync(model.Id);
            // if(product is null)
            //     return new Response(false, "Product not found");
            _dbContext.Products.Update(model);
            var changes = await _dbContext.SaveChangesAsync();
            if (changes > 0)
            {
                _logger.LogInformation("Product updated successfully: {ProductName}", model.Name);
                return new Response(true, $"{model.Name} is updated successfully");
            }
            else
            {
                _logger.LogWarning("No changes made when updating product: {ProductName}", model.Name);
                return new Response(false, "No changes detected");
            }
        }
        catch (DbUpdateConcurrencyException concurrencyEx)
        {
            _logger.LogError(concurrencyEx, "Concurrency conflict updating product: {ProductId}", model.Id);
            return new Response(false, "Product was modified by another user. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error updating product: {ProductName}", model.Name);
            return new Response(false, "Database error occurred while updating the product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating product: {ProductName}", model.Name);
            throw new InvalidOperationException("Error occured while detecting the product");
        }
    }

    public async Task<Response> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            var product = await FindByIdAsync(id);
            if (product is null)
            {
                _logger.LogWarning("Product not found for deletion: {ProductId}", id);
                return new Response(false, "Product not found");
            }
            _dbContext.Products.Remove(product);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Product deleted successfully: {ProductName} with ID: {ProductId}", 
                    product.Name, id);
                return new Response(true, $"{product.Name} is deleted successfully");
            }
            else
            {
                _logger.LogWarning("No changes made when deleting product: {ProductId}", id);
                return new Response(false, "No changes detected when deleting product");
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error deleting product: {ProductId}", id);
            return new Response(false, "Database error occurred while deleting the product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting product: {ProductId}", id);
            return new Response(false, "Error occured while delecting the Product");
        }
    }
}
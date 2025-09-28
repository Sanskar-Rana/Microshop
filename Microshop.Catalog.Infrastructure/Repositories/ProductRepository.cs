using System.Linq.Expressions;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Infrastructure.Data;
using Microshop.SharedLibrary.Logs;
using Microshop.SharedLibrary.Response;
using Microsoft.EntityFrameworkCore;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Product> GetByAsync(Expression<Func<Product, bool>> predicate)
    {
        try
        {
            var product = await _dbContext.Products.Where(predicate).FirstOrDefaultAsync();
            return product is not null ? product : null;
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while detecting the product");
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            var products = await _dbContext.Products.ToListAsync();
            return products is not null ? products : null!;
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while detecting the product");
        }
    }

    public async Task<Product> FindByIdAsync(Guid id)
    {
        try
        {
            var product = await _dbContext.Products.FindAsync(id);
            return product is not null ? product : null;
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while detecting the product");
            
        }
    }

    public async Task<Response> CreateAsync(Product model)
    {
        try
        {
            var getProduct = await GetByAsync(_ => _.Name!.Equals( model.Name));
            if (getProduct is not null && !string.IsNullOrWhiteSpace(getProduct.Name))
                return new Response(false, "Product already exists");
            
            var currentEntity = _dbContext.Products.Add(model);
            await _dbContext.SaveChangesAsync();

            if (currentEntity is not null)
                return new Response(true, $"{model.Name} added to database successfully");
            else
            {
                return new Response(false, $"{model.Name} could not be added");
            }
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            return new Response(false, $"{model.Name} could not be added");
        }
    }

    public async Task<Response> UpdateAsync(Product model)
    {
        try
        {
            var product = await FindByIdAsync(model.Id);
            if(product is null)
                return new Response(false, "Product not found");
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();

            return new Response(true, $"{model.Name} is updated succesfully");
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while detecting the product");
        }
    }

    public async Task<Response> DeleteAsync(Guid id)
    {
        try
        {
            var product = await FindByIdAsync(id);
            if (product is null)
                return new Response(false, $"{product.Name} not found");
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return new Response(true, $"{product.Name} deleted successfully");
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            return new Response(false, "Error occured while delecting the Product");
        }
    }
}
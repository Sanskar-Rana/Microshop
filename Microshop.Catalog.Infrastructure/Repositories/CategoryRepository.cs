using System.Linq.Expressions;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Infrastructure.Data;
using Microshop.SharedLibrary.Logs;
using Microshop.SharedLibrary.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly  AppDbContext _dbContext;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(AppDbContext dbContext, ILogger<CategoryRepository> logger)
    {
        _dbContext =  dbContext;
        _logger = logger;
    }
    public async Task<Category> GetByAsync(Expression<Func<Category, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("Getting category by id");
            var category = await _dbContext.Categories.Where(predicate).Include(p => p.Products).FirstOrDefaultAsync();
            if (category == null)
            {
                _logger.LogDebug("Category not found with the specified predicate");
                return null!;
            }
            _logger.LogDebug("Category retrieved successfully: {CategoryId}", category.Id);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving category by predicate");
            throw new InvalidOperationException("Error occured while delecting the category");
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all categories");
            var categories = await _dbContext.Categories.AsNoTracking().Include(p => p.Products).ToListAsync();
            _logger.LogDebug("Retrieved {CategoryCount} categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all categories");
            throw new InvalidOperationException("Error occured while delecting the category");
        }
    }

    public async Task<Category> FindByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Finding category by ID: {CategoryId}", id);
            var category = await _dbContext.Categories.Include(p => p.Products).FirstOrDefaultAsync(p => p.Id == id);
            if (category == null)
            {
                _logger.LogDebug("Category not found: {CategoryId}", id);
                return null!;
            }

            _logger.LogDebug("Category found: {CategoryId}", id);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while finding category: {CategoryId}", id);
            throw new InvalidOperationException("Error occured while delecting the category");
        }
    }

    public async Task<Response> CreateAsync(Category model)
    {
        try
        {
            _logger.LogInformation("Creating new category: {CategoryName}", model.Name);
            var getCategory = await GetByAsync(_ => _.Name!.Equals( model.Name));
            if (getCategory is not null && !string.IsNullOrWhiteSpace(getCategory.Name)) 
            {
                _logger.LogWarning("Category already exists: {CategoryName}", model.Name);
                return new Response(false, "Category already exists");
            }
            
            var currentEntity = _dbContext.Categories.Add(model).Entity;
            await _dbContext.SaveChangesAsync();

            if (currentEntity is not null)
            {
                _logger.LogInformation("Category created successfully: {CategoryName} with ID: {CategoryId}", 
                    model.Name, model.Id);
                return new Response(true, $"{model.Name} added to database successfully");
            }
            else
            {
                _logger.LogWarning("Category creation failed - no entity returned: {CategoryName}", model.Name);
                return new Response(false, $"{model.Name} could not be added");
            }

        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error creating category: {CategoryName}", model.Name);
            return new Response(false, "Database error occurred while adding new Category");
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            return new Response(false, "Error occured while adding new Category");
        }
    }

    public async Task<Response> UpdateAsync(Category model)
    {
        try
        {
            _logger.LogInformation("Updating category: {CategoryName} with ID: {CategoryId}", model.Name, model.Id);
           // var category = await FindByIdAsync(model.Id);
           // if(category is null)
           //     return new Response(false, $"{model.Name} not found");
           // _dbContext.Entry(model).State = EntityState.Detached;
            _dbContext.Categories.Update(model);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Category updated successfully: {CategoryName}", model.Name);
                return new Response(true, $"{model.Name} is updated successfully");
            }
            else
            {
                _logger.LogWarning("No changes made when updating category: {CategoryName}", model.Name);
                return new Response(false, "No changes detected");
            }
        }
        catch (DbUpdateConcurrencyException concurrencyEx)
        {
            _logger.LogError(concurrencyEx, "Concurrency conflict updating category: {CategoryId}", model.Id);
            return new Response(false, "Category was modified by another user. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error updating category: {CategoryName}", model.Name);
            return new Response(false, "Database error occurred while updating the category");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating category: {CategoryName}", model.Name);
            return new Response(false, "Error occured while updating the category");
        }
    }

    public async Task<Response> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting category with ID: {CategoryId}", id);
            var category = await FindByIdAsync(id);
            if (category is null)   
            {
                _logger.LogWarning("Category not found for deletion: {CategoryId}", id);
                return new Response(false, "Category not found");
            }
            _dbContext.Categories.Remove(category);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Category deleted successfully: {CategoryName} with ID: {CategoryId}", 
                    category.Name, id);
                return new Response(true, $"{category.Name} is deleted successfully");
            }
            else
            {
                _logger.LogWarning("No changes made when deleting category: {CategoryId}", id);
                return new Response(false, "No changes detected when deleting category");
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error deleting category: {CategoryId}", id);
            return new Response(false, "Database error occurred while deleting the category");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting category: {CategoryId}", id);
            return new Response(false, "Error occured while delecting the category");
        }
    }
}
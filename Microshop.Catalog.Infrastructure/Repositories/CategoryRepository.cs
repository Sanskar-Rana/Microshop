using System.Linq.Expressions;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Infrastructure.Data;
using Microshop.SharedLibrary.Logs;
using Microshop.SharedLibrary.Response;
using Microsoft.EntityFrameworkCore;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly  AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext =  dbContext;
    }
    public async Task<Category> GetByAsync(Expression<Func<Category, bool>> predicate)
    {
        try
        {
            var category = await _dbContext.Categories.Where(predicate).FirstOrDefaultAsync();
            return category is not null ? category : null!;
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while delecting the category");
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            var categories = await _dbContext.Categories.AsNoTracking().ToListAsync();
            return categories is not null ? categories : null!;
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while delecting the category");
        }
    }

    public async Task<Category> FindByIdAsync(Guid id)
    {
        try
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
            return category is not null ? category : null!;
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            throw new InvalidOperationException("Error occured while delecting the category");
        }
    }

    public async Task<Response> CreateAsync(Category model)
    {
        try
        {
            var getCategory = await GetByAsync(_ => _.Name!.Equals( model.Name));
            if (getCategory is not null && !string.IsNullOrWhiteSpace(getCategory.Name)) 
                return new Response(false, "Category already exists");
            
            var currentEntity = _dbContext.Categories.Add(model).Entity;
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
            return new Response(false, "Error occured while adding new Category");
        }
    }

    public async Task<Response> UpdateAsync(Category model)
    {
        try
        {
           // var category = await FindByIdAsync(model.Id);
           // if(category is null)
           //     return new Response(false, $"{model.Name} not found");
           // _dbContext.Entry(model).State = EntityState.Detached;
            _dbContext.Categories.Update(model);
            await _dbContext.SaveChangesAsync();

            return new Response(true, $"{model.Name} is updated successfully");
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            return new Response(false, "Error occured while updating the category");
        }
    }

    public async Task<Response> DeleteAsync(Guid id)
    {
        try
        {
            var category = await FindByIdAsync(id);
            if (category is null)
                return new Response(false, $"{category.Name} not found");
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return new Response(true, $"{category.Name} is deleted successfully");
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);
            return new Response(false, "Error occured while delecting the category");
        }
    }
}
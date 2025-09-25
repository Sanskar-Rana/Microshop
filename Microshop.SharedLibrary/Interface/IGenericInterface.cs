using System.Linq.Expressions;

namespace Microshop.SharedLibrary.Interface;

public interface IGenericInterface<T> where T : class
{
    Task<T> GetByAsync(Expression <Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> FindByIdAsync(Guid id);
    
    Task<Response.Response> CreateAsync(T model);
    Task<Response.Response> UpdateAsync(T model);
    Task<Response.Response> DeleteAsync(Guid id);
    
    
}
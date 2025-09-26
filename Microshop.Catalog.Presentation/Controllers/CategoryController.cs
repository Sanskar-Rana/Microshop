using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Microshop.Catalog.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly CategoryRepository _repository;
    private readonly CategoryValidator _validator;
    public CategoryController(CategoryRepository repository, CategoryValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategoryAsync()
    {
        var categories = await _repository.GetAllAsync();
        if(categories == null)
            return NotFound();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> AddCategoryAsync(Category category)
    {
        if(!ModelState.IsValid)
            return BadRequest();
        
        var validators = await _validator.ValidateAsync(category);
        if (!validators.IsValid)
            return BadRequest(Results.ValidationProblem(validators.ToDictionary()));

        var response = await _repository.CreateAsync(category);
        if(response.Flag is true)
            return Ok(response);
        else
        {
            return BadRequest(response);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Category>> GetCategoryAsync(Guid id)
    {
        var category = await _repository.FindByIdAsync(id);
        if(category == null)
            return NotFound();
        return Ok(category);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Category>> UpdateCategoryAsync(Guid id, Category category)
    {
        var  validators = await _validator.ValidateAsync(category);
        if(!validators.IsValid)
            return BadRequest(Results.ValidationProblem(validators.ToDictionary()));
        var dbCategory = await _repository.FindByIdAsync(id);
        
        if(dbCategory == null)
            return NotFound();
        
        var response = await _repository.UpdateAsync(dbCategory);
        
        if(response.Flag is true)
            return Ok(response);
        else
        {
            return BadRequest(response);
        }
        
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Category>> DeleteCategoryAsync(Guid id)
    {
        var category = await _repository.FindByIdAsync(id);
        if(category == null)
            return NotFound();
        var response = await _repository.DeleteAsync(id);
        if(response.Flag is true)
            return Ok(response);
        else
        {
            return BadRequest(response);
        }
    }

}
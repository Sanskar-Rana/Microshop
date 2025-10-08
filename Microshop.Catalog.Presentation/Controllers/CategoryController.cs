using AutoMapper;
using FluentValidation;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Micrsoshop.Catalog.Application.Dtos.Category;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoryController : ControllerBase
{
    private readonly ICategoryRepository _repository;
    private readonly IValidator<Category> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryController> _logger;
    public CategoryController(ICategoryRepository repository, IValidator<Category> validator, IMapper mapper, ILogger<CategoryController> logger)
    {
        _repository = repository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
        
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetAllCategoryAsync()
    {
        using var scope = _logger.BeginScope("Get all categories");
        _logger.LogInformation("Getting all categories");
        try
        {
            var categories = await _repository.GetAllAsync();
            if (categories == null)
            {
                _logger.LogWarning("No categories found - returning empty list");
                return NotFound();
            }

            _logger.LogInformation("Retrieved {CategoryCount} categories successfully", categories.Count());
            return Ok(_mapper.Map<IEnumerable<CategoryReadDto>>(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving all categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
        
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryCreateDto>> AddCategoryAsync(CategoryCreateDto category)
    {
        using var scope = _logger.BeginScope("Create new category with {@Category}", category);
        _logger.LogInformation("Adding new category: { @category}", category);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state: {@ModelState}",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ModelState);

        }

        try
        {
            //DTO
            var model = _mapper.Map<Category>(category);

            //Validate the domain entry
            var validators = await _validator.ValidateAsync(model);
            if (!validators.IsValid)
            {
                _logger.LogWarning("Validation failed for category: {ValidationErrors}",
                    validators.Errors.Select(e => e.ErrorMessage));
                return BadRequest(Results.ValidationProblem(validators.ToDictionary()));

            }

            //Save using repository
            var response = await _repository.CreateAsync(model);

            if (response.Flag is true)
            {
                _logger.LogInformation("Category created successfully: {CategoryId}", model.Id);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Failed to create category: {Message}", response.Message);
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating category: {@Category}", category);
            return StatusCode(500, "An error occurred while creating the category");
        }


    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategoryAsync(Guid id)
    {
        using var scope = _logger.BeginScope("Get category {Id}", id);
        _logger.LogInformation("Getting category by ID: {CategoryId}", id);
        try
        {
            var category = await _repository.FindByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", id);
                return NotFound();
            }

            _logger.LogInformation("Category retrieved successfully: {CategoryId}", category.Id);
            return Ok(_mapper.Map<CategoryReadDto>(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving category: {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving category");
        }


    }

    [HttpPut("{id:guid}")]
   [Authorize(Roles = "Admin,User")]
    public async Task<ActionResult<CategoryCreateDto>> UpdateCategoryAsync(Guid id, CategoryCreateDto category)
    {
        using var scope = _logger.BeginScope("Update category {Id}", id);
        _logger.LogInformation("Updating category: {CategoryId} with data: {@CategoryData}", id, category);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for category update: {CategoryId}", id);
            return BadRequest(ModelState);
        }

        try
        {
            var dbCategory = await _repository.FindByIdAsync(id);
            if (dbCategory == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", id);
                return NotFound();
            }

            // map DTO → existing entity (preserves Id, CreatedAt)
            _mapper.Map(category, dbCategory);
            dbCategory.UpdatedAt = DateTime.Now;

            var validationResult = await _validator.ValidateAsync(dbCategory);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for category update: {CategoryId}, Errors: {ValidationErrors}",
                    id, validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(Results.ValidationProblem(validationResult.ToDictionary()));
            }


            var response = await _repository.UpdateAsync(dbCategory);

            if (response.Flag)
            {
                _logger.LogInformation("Category updated successfully: {CategoryId}", id);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Failed to update category: {CategoryId}, Message: {Message}",
                    id, response.Message);
                return BadRequest(response);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating category: {CategoryId}", id);
            return StatusCode(500, "An error occured while updating the category");
        }
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Category>> DeleteCategoryAsync(Guid id)
    {
        using var scope = _logger.BeginScope("Delete category {Id}", id);
        _logger.LogInformation("Deleting category: {CategoryId}", id);
        try
        {
            var category = await _repository.FindByIdAsync(id);
            if (category == null)
                return NotFound();
            var response = await _repository.DeleteAsync(id);
            if (response.Flag is true)
                return Ok(response);
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the category");
        }


    }

}
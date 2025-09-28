using AutoMapper;
using FluentValidation;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Repositories;
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
    private readonly IValidator<CategoryCreateDto> _validatorCategoryReadDto;
    private readonly IMapper _mapper;
    public CategoryController(ICategoryRepository repository, IValidator<Category> validator, IMapper mapper, IValidator<CategoryCreateDto> validatorCategoryReadDto)
    {
        _repository = repository;
        _validator = validator;
        _mapper = mapper;
        _validatorCategoryReadDto = validatorCategoryReadDto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetAllCategoryAsync()
    {
        var categories = await _repository.GetAllAsync();
        if(categories == null)
            return NotFound();
        return Ok( _mapper.Map<IEnumerable<CategoryReadDto>>(categories));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryCreateDto>> AddCategoryAsync(CategoryCreateDto category)
    {
        if(!ModelState.IsValid)
            return BadRequest();
        
        //DTO
        var model = _mapper.Map<Category>(category);
        
        //Validate the domain entry
        var validators = await _validator.ValidateAsync(model);
        if (!validators.IsValid)
            return BadRequest(Results.ValidationProblem(validators.ToDictionary()));
        
        //Save using repository
        var response = await _repository.CreateAsync(model);
        
        if(response.Flag is true)
            return Ok(response);
        else
        {
            return BadRequest(response);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategoryAsync(Guid id)
    {
        var category = await _repository.FindByIdAsync(id);
        if(category == null)
            return NotFound();
        return Ok(_mapper.Map<CategoryReadDto>(category));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryCreateDto>> UpdateCategoryAsync(Guid id, CategoryCreateDto category)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dbCategory = await _repository.FindByIdAsync(id);
        if (dbCategory == null)
            return NotFound();

        // map DTO → existing entity (preserves Id, CreatedAt)
        _mapper.Map(category, dbCategory);

        dbCategory.UpdatedAt = DateTime.Now;
        
        var validationResult = await _validator.ValidateAsync(dbCategory);
        if (!validationResult.IsValid)
            return BadRequest(Results.ValidationProblem(validationResult.ToDictionary()));

        var response = await _repository.UpdateAsync(dbCategory);

        return response.Flag ? Ok(response) : BadRequest(response);
        
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
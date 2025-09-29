using AutoMapper;
using FluentValidation;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Micrsoshop.Catalog.Application.Dtos.Product;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{

   private readonly IProductRepository _repository;
   private readonly IValidator<Product>  _validator;
   private readonly IMapper _mapper;
   
   public ProductController(IProductRepository repository, IValidator<Product> validator, IMapper mapper)
   {
      _repository = repository;
      _validator = validator;
      _mapper = mapper;
   }

   [HttpGet]
   public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAllProductAsync()
   {
      var products = await _repository.GetAllAsync();
      if (products == null)
         return NotFound();
      return Ok(_mapper.Map<IEnumerable<ProductReadDto>>(products));
   }
   
   [HttpPost]
   public async Task<ActionResult<ProductReadDto>> AddProductAsync(ProductCreateDto product)
   {
      if(!ModelState.IsValid)
         return BadRequest();
      
      var model = _mapper.Map<Product>(product);
      
      var validationResult = await _validator.ValidateAsync(model);
      if(!validationResult.IsValid)
         return BadRequest(Results.ValidationProblem(validationResult.ToDictionary()));

      var response = await _repository.CreateAsync(model);
      if (response.Flag is true)
         return Ok(response);
      else
         return BadRequest(response);
   }

   [HttpGet("{id:guid}")]
   public async Task<ActionResult<ProductReadDto>> GetProductAsync(Guid id)
   {
      var product = await _repository.FindByIdAsync(id);
      if (product == null)
         return NotFound();
      return Ok(_mapper.Map<ProductReadDto>(product));
   }

   [HttpPut("{id:guid}")]
   public async Task<ActionResult<ProductReadDto>> UpdateProductAsync(Guid id, ProductCreateDto product)
   {
      if (!ModelState.IsValid)
         return BadRequest();
      
      var dbProduct = await _repository.FindByIdAsync(id);
      if(dbProduct == null)
         return NotFound();
      
      _mapper.Map(product, dbProduct);
      dbProduct.UpdatedAt = DateTime.Now;
      
      var  validatorsResult = await _validator.ValidateAsync(dbProduct);
      if(!validatorsResult.IsValid)
         return BadRequest(Results.ValidationProblem(validatorsResult.ToDictionary()));
      
      var response = await _repository.UpdateAsync(dbProduct);
        
      if(response.Flag is true)
         return Ok(response);
      else
      {
         return BadRequest(response);
      }
   }

   [HttpDelete("{id:guid}")]
   public async Task<IActionResult> DeleteProductAsync(Guid id)
   {
      var product = await _repository.FindByIdAsync(id);
      if(product == null)
         return NotFound();
      var response = await _repository.DeleteAsync(id);
      if (response.Flag is true)
         return Ok(response);
      else
      {
         return BadRequest(response);
      }
   }

}
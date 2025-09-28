using FluentValidation;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{

   private readonly IProductRepository _repository;
   private readonly IValidator<Product>  _validator;
   
   public ProductController(IProductRepository repository, IValidator<Product> validator)
   {
      _repository = repository;
      _validator = validator;
   }

   [HttpGet]
   public async Task<IActionResult> GetAllProductAsync()
   {
      var products = await _repository.GetAllAsync();
      if (products == null)
         return NotFound();
      return Ok(products);
   }


   [HttpPost]
   public async Task<ActionResult<Product>> AddProductAsync(Product product)
   {
      if(!ModelState.IsValid)
         return BadRequest();
      
      var validationResult = await _validator.ValidateAsync(product);
      if(!validationResult.IsValid)
         return BadRequest(Results.ValidationProblem(validationResult.ToDictionary()));

      var response = await _repository.CreateAsync(product);
      if (response.Flag is true)
         return Ok(response);
      else
         return BadRequest(response);
   }

   [HttpGet("{id:guid}")]
   public async Task<ActionResult<Product>> GetProductAsync(Guid id)
   {
      var product = await _repository.FindByIdAsync(id);
      if (product == null)
         return NotFound();
      return Ok(product);
   }

   [HttpPut("{id:guid}")]
   public async Task<ActionResult<Product>> UpdateProductAsync(Guid id, Product product)
   {
      var  validatorsResult = await _validator.ValidateAsync(product);
      if(!validatorsResult.IsValid)
         return BadRequest(Results.ValidationProblem(validatorsResult.ToDictionary()));
      var dbProduct = await _repository.FindByIdAsync(id);
        
      if(dbProduct == null)
         return NotFound();
        
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
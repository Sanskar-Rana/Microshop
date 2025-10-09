using AutoMapper;
using FluentValidation;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
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
   private readonly ILogger<ProductController> _logger;
   
   public ProductController(IProductRepository repository, IValidator<Product> validator, IMapper mapper, ILogger<ProductController> logger)
   {
      _repository = repository;
      _validator = validator;
      _mapper = mapper;
      _logger = logger;
   }

   [HttpGet]
   public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAllProductAsync()
   {
      using var scope = _logger.BeginScope("GetAllProductAsync");
      _logger.LogInformation("Getting all products");
      try
      {
         var products = await _repository.GetAllAsync();
         if (products == null || !products.Any())
         {
            _logger.LogWarning("No products found - returned empty collection");
            return NotFound();
         }

         _logger.LogInformation("Retrieved {ProductCount} products successfully", products.Count());
         return Ok(_mapper.Map<IEnumerable<ProductReadDto>>(products));
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error retrieving all products");
         return StatusCode(500, "An error occurred while retrieving products");
      }
   }
   
   [HttpPost]
   public async Task<ActionResult<ProductReadDto>> AddProductAsync(ProductCreateDto product)
   {
      using  var scope = _logger.BeginScope("Add product");
      _logger.LogInformation("Creating new product: {@Product}", product);

      if (!ModelState.IsValid)
      {
         _logger.LogWarning("Invalid model state for product creation: {@ModelState}", 
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
         return BadRequest(ModelState);
      }

      try
      {
         var model = _mapper.Map<Product>(product);

         var validationResult = await _validator.ValidateAsync(model);
         if (!validationResult.IsValid)
         {
            _logger.LogWarning("Validation failed for product: {ValidationErrors}", 
               validationResult.Errors.Select(e => e.ErrorMessage));
            return BadRequest(Results.ValidationProblem(validationResult.ToDictionary()));
         }

         var response = await _repository.CreateAsync(model);
         if (response.Flag is true)
         {
            _logger.LogInformation("Product created successfully: {ProductId}", model.Id);
            return Ok(response);
         }
         else
         {
            _logger.LogWarning("Failed to create product: {Message}", response.Message);
            return BadRequest(response);
         }
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error creating product: {@Product}", product);
         return StatusCode(500, "An error occurred while creating the product");
      }


   }

   [HttpGet("{id:guid}")]
   public async Task<ActionResult<ProductReadDto>> GetProductAsync(Guid id)
   {
      using var scope = _logger.BeginScope("Get product {Id}", id);
      _logger.LogInformation("Getting product by ID: {ProductId}", id);

      try
      {
         var product = await _repository.FindByIdAsync(id);
         if (product == null)
         {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound();
         }

         _logger.LogInformation("Product retrieved successfully: {ProductId}", id);
         return Ok(_mapper.Map<ProductReadDto>(product));
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error retrieving product: {ProductId}", id);
         return StatusCode(500, "An error occurred while retrieving the product");
      }

     
   }

   [HttpPut("{id:guid}")]
   [Authorize(Roles = "Admin")]
   public async Task<ActionResult<ProductReadDto>> UpdateProductAsync(Guid id, ProductCreateDto product)
   {
      using var scope =  _logger.BeginScope("Update product {Id}", id);
      _logger.LogInformation("Updating product: {ProductId} with data: {@ProductData}", id, product);
      if (!ModelState.IsValid)
      {
         _logger.LogWarning("Invalid model state for product update: {ProductId}", id);
         return BadRequest(ModelState);
      }

      try
      {
         var dbProduct = await _repository.FindByIdAsync(id);
         if (dbProduct == null)
         {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            return NotFound();
         }

         _mapper.Map(product, dbProduct);
         dbProduct.UpdatedAt = DateTime.Now;

         var validatorsResult = await _validator.ValidateAsync(dbProduct);
         if (!validatorsResult.IsValid)
         {
            _logger.LogWarning("Validation failed for product update: {ProductId}, Errors: {ValidationErrors}", 
               id, validatorsResult.Errors.Select(e => e.ErrorMessage));
            return BadRequest(Results.ValidationProblem(validatorsResult.ToDictionary()));
         }

         var response = await _repository.UpdateAsync(dbProduct);

         if (response.Flag)
         {
            _logger.LogInformation("Product updated successfully: {ProductId}", id);
            return Ok(response);
         }
         else
         {
            _logger.LogWarning("Failed to update product: {ProductId}, Message: {Message}", 
               id, response.Message);
            return BadRequest(response);
         }
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error updating product: {ProductId}", id);
         return StatusCode(500, "An error occurred while updating the product");
      }


   }

   [HttpDelete("{id:guid}")]
   [Authorize(Roles = "Admin")]
   public async Task<IActionResult> DeleteProductAsync(Guid id)
   {
      using var scope = _logger.BeginScope("Delete product {Id}", id);
      _logger.LogInformation("Deleting product: {ProductId}", id);

      try
      {
         var product = await _repository.FindByIdAsync(id);
         if (product == null)
         {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            return NotFound();
         }
         var response = await _repository.DeleteAsync(id);
         if (response.Flag)
         {
            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            return Ok(response);
         }
         else
         {
            _logger.LogWarning("Failed to delete product: {ProductId}, Message: {Message}", 
               id, response.Message);
            return BadRequest(response);
         }
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error deleting product: {ProductId}", id);
         return StatusCode(500, "An error occurred while deleting the product");
      }

     
   }

}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Warehouse.Models; 
using Warehouse.Data; 

[ApiController]
[Route("[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WarehouseController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductMovementDto productMovement)
    {
       
        if (productMovement.Amount <= 0)
        {
            return BadRequest("The amount must be greater than zero.");
        }

        
        var product = await _context.Products.FindAsync(productMovement.ProductId);
        var warehouse = await _context.Warehouses.FindAsync(productMovement.WarehouseId);
        if (product == null || warehouse == null)
        {
            return NotFound("Product or Warehouse not found.");
        }

       
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.IdProduct == productMovement.ProductId && 
                                      o.Amount == productMovement.Amount &&
                                      o.CreatedAt < productMovement.CreatedAt);
        if (order == null)
        {
            return BadRequest("No matching order found, or order creation date is invalid.");
        }

    
        var isFulfilled = await _context.ProductWarehouses
            .AnyAsync(pw => pw.IdOrder == order.Id);
        if (isFulfilled)
        {
            return BadRequest("Order has already been fulfilled.");
        }

     
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                
                order.FullfilledAt = DateTime.UtcNow; 
                _context.Orders.Update(order);

                
                var productWarehouse = new ProductWarehouse
                {
                    IdOrder = order.Id, 
                    IdProduct = productMovement.ProductId,
                    IdWarehouse = productMovement.WarehouseId,
                    Amount = productMovement.Amount,
                    Price = product.Price * productMovement.Amount, 
                    CreatedAt = DateTime.UtcNow
                };
                await _context.ProductWarehouses.AddAsync(productWarehouse);

               
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                
                return Ok(productWarehouse.Id);
            }
            catch (Exception ex)
            {
              
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
    [HttpPost("AddProductToWarehouseWithProcedure")]
    public async Task<IActionResult> AddProductToWarehouseWithProcedure([FromBody] ProductMovementDto productMovement)
    {
       
        var productIdParameter = new SqlParameter("@ProductId", productMovement.ProductId);
        var warehouseIdParameter = new SqlParameter("@WarehouseId", productMovement.WarehouseId);
        var amountParameter = new SqlParameter("@Amount", productMovement.Amount);
        var createdAtParameter = new SqlParameter("@CreatedAt", productMovement.CreatedAt);

       
        var outputParameter = new SqlParameter
        {
            ParameterName = "@NewProductWarehouseId",
            SqlDbType = System.Data.SqlDbType.Int,
            Direction = System.Data.ParameterDirection.Output
        };

       
        await _context.Database.ExecuteSqlRawAsync("EXEC usp_AddProductToWarehouse @ProductId, @WarehouseId, @Amount, @CreatedAt, @NewProductWarehouseId OUT", 
            productIdParameter, warehouseIdParameter, amountParameter, createdAtParameter, outputParameter);

       
        var newProductWarehouseId = (int)outputParameter.Value;

      
        if (newProductWarehouseId > 0)
        {
            return Ok(newProductWarehouseId);
        }
        else
        {
            return BadRequest("Unable to add product to warehouse using the stored procedure.");
        }
    }
}



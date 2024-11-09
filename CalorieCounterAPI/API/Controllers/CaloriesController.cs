using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using DAL;

namespace CalorieCounterAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaloriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CaloriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Day>>> GetProductsPerDay()
        {
            var products = await _unitOfWork.DayRepository.GetAllAsync(query =>
                query.Include(d => d.Products));

            if (!products.Any())
            {
                return NotFound("No products found.");
            }

            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIDAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (product.Day == null)
            {
                // Try to find an existing Day for today's date.
                var today = DateTime.Today;
                var existingDay = await _unitOfWork.DayRepository.FindAsync(d => d.Date == today);

                // If no Day exists for today, create and save a new Day.
                if (existingDay == null)
                {
                    existingDay = new Day
                    {
                        Date = today
                    };
                    await _unitOfWork.DayRepository.InsertAsync(existingDay);
                    await _unitOfWork.SaveAsync();
                }

                // Assign the found or newly created Day to the product.
                product.Day = existingDay;
            }

            await _unitOfWork.ProductRepository.InsertAsync(product);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest("Product ID mismatch.");
            }

            var existingProduct = await _unitOfWork.ProductRepository
                .FindAsync(p => p.ProductId == id, include => include.Include(p => p.Day));

            if (existingProduct == null)
            {
                return NotFound();
            }

            // Now update the product with the new one
            await _unitOfWork.ProductRepository.UpdateAsync(existingProduct);
            await _unitOfWork.SaveAsync();

            return NoContent();  // 204 No Content to indicate successful update
        }



        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIDAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            await _unitOfWork.ProductRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}

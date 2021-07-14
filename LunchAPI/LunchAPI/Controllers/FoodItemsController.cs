using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Models;
using lunch_project.Classes;
using Repositories;

namespace LunchAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        private readonly FoodItemsRepository repo;


        public FoodItemsController()
        {
            repo = new FoodItemsRepository();
            
        }

        // GET: api/FoodItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItems()
        {
            return await repo.GetFoodItems();
        }

        // GET: api/FoodItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodItem>> GetFoodItem(string id)
        {
            var foodItem = await repo.FindAsync(id);

            if (foodItem == null)
            {
                return NotFound();
            }

            return foodItem;
        }

        // GET: api/FoodItems/School/5
        [HttpGet("School/{schoolID}")]
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItemsBySchool(string schoolID)
        {
            var allFoodItems = await repo.GetFoodItems();
            var requestedFoodItems = allFoodItems.Value.Where(p => string.Equals(p.SchoolID, schoolID, StringComparison.OrdinalIgnoreCase));
            return new ActionResult<IEnumerable<FoodItem>>(requestedFoodItems); 
        }

        // PUT: api/FoodItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodItem(string id, FoodItem foodItem)
        {
            if (id != foodItem.ID)
            {
                return BadRequest();
            }

            repo.ModifiedEntityState(foodItem);

            try
            {
                await repo.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!repo.FoodItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/FoodItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<FoodItem>> PostFoodItem(FoodItem foodItem)
        {
            repo.Add(foodItem);
            try
            {
                await repo.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (repo.FoodItemExists(foodItem.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetFoodItem", new { id = foodItem.ID }, foodItem);
        }

        // DELETE: api/FoodItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(string id)
        {
            var foodItem = await repo.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            repo.Remove(foodItem);
            await repo.SaveChangesAsync();

            return NoContent();
        }

    }
}

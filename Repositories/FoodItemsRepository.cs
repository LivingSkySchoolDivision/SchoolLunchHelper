using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using lunch_project.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Repositories
{
    /**<summary>Separates EF Core code from the FoodItem controller</summary>
     */
    public class FoodItemsRepository
    {
        private readonly DataDbContext _context = ContextInjector.Context;

        public FoodItemsRepository() 
        {
        }

        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItems()
        {
            return await _context.FoodItems.ToListAsync();
        }

        /*
        public async Task<ActionResult<FoodItem>> GetFoodItem(string id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);

            return foodItem;
        }
        */

        /*
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItemFromCategory(string category)
        {
            //var foodItems = _context.FoodItems.Where(s => s.SchoolID.Equals(category));
            return new ActionResult<IEnumerable<FoodItem>>(foodItems);
        }
        */

        public async Task<FoodItem> FindAsync(string id)
        {
            return await _context.FoodItems.FindAsync(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void ModifiedEntityState(FoodItem foodItem)
        {
            _context.Entry(foodItem).State = EntityState.Modified;
        }

        public void Add(FoodItem foodItem)
        {
            _context.FoodItems.Add(foodItem);
        }

        public EntityEntry<FoodItem> Remove(FoodItem foodItem)
        {
            return _context.FoodItems.Remove(foodItem);
        }

        public bool FoodItemExists(string id)
        {
            return _context.FoodItems.Any(e => e.ID == id);
        }

    }
}

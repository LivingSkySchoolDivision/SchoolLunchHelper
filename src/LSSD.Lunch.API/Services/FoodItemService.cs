using System;
using System.Collections.Generic;

namespace LSSD.Lunch.API.Services
{
    public class FoodItemService 
    {
        private readonly IRepository<FoodItem> _repository;        

        public FoodItemService (IRepository<FoodItem> Repository) 
        {
            this._repository = Repository;            
        }

        public FoodItem Get(Guid GUID) 
        {
            return _repository.GetById(GUID);
        }

        public IEnumerable<FoodItem> GetAll() 
        {
            return _repository.GetAll();            
        }
        
        public void InsertOrUpdate(FoodItem FoodItem) 
        {
            _repository.Update(FoodItem);
        }
    }

}
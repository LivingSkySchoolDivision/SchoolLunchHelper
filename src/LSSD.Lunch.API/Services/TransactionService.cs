using System;
using System.Collections.Generic;

namespace LSSD.Lunch.API.Services
{
    public class TransactionService 
    {
        private readonly IRepository<Transaction> _repository;        

        public TransactionService (IRepository<Transaction> Repository) 
        {
            this._repository = Repository;            
        }

        public Transaction Get(Guid GUID) 
        {
            return _repository.GetById(GUID);
        }

        public IEnumerable<Transaction> GetAll() 
        {
            return _repository.GetAll();            
        }

        public void InsertOrUpdate(Transaction Transaction) 
        {
            _repository.Update(Transaction);
        }
        
    }

}
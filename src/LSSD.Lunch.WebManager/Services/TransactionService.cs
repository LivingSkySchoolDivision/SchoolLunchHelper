using System;
using System.Collections.Generic;

namespace LSSD.Lunch.WebManager.Services
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

        public Transaction Get(string GUID) 
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

        public void Delete(Transaction item) {
            _repository.Delete(item);
        }
        
    }

}
using System;
using System.Linq;
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

        public void Delete(Transaction item) 
        {
            _repository.Delete(item);
        }

        public IEnumerable<Transaction> GetForStudentID(string StudentId) 
        {
            return _repository.Find(x => x.StudentNumber == StudentId);
        }

        public IEnumerable<Transaction> GetForStudentGUID(Guid StudentGuid) 
        {            
            return _repository.Find(x => x.StudentID == StudentGuid);
        }

        public IEnumerable<Transaction> GetForStudentGUID(string StudentGuid) 
        {
            Guid id = Guid.Parse(StudentGuid);
            List<Transaction> results = _repository.Find(x => x.StudentID == id).ToList();            
            return results;
        }
        
        public IEnumerable<Transaction> GetForStudentGUID(Guid StudentGuid, DateTime StartDateUTC, DateTime EndDateUTC) 
        {
            return _repository.Find(x => x.Id == StudentGuid && x.TimestampUTC >= StartDateUTC && x.TimestampUTC <= EndDateUTC);
        }
        
    }

}
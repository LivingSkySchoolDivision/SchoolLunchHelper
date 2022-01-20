using System;
using System.Collections.Generic;

namespace LSSD.Lunch.API.Services
{
    public class SchoolService 
    {
        private readonly IRepository<School> _repository;        

        public SchoolService (IRepository<School> Repository) 
        {
            this._repository = Repository;            
        }

        public School Get(Guid GUID) 
        {
            return _repository.GetById(GUID);
        }

        public IEnumerable<School> GetAll() 
        {
            return _repository.GetAll();            
        }
        
        public void InsertOrUpdate(School school) 
        {
            _repository.Update(school);
        }
    }

}
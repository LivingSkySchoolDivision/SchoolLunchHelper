using System;
using System.Collections.Generic;

namespace LSSD.Lunch.API.Services
{
    public class StudentService 
    {
        private readonly IRepository<Student> _repository;        

        public StudentService (IRepository<Student> Repository) 
        {
            this._repository = Repository;            
        }

        public Student Get(Guid GUID) 
        {
            return _repository.GetById(GUID);
        }

        public IEnumerable<Student> GetAll() 
        {
            return _repository.GetAll();            
        }
        public void InsertOrUpdate(Student Student) 
        {
            _repository.Update(Student);
        }
        
    }

}
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSSD.Lunch.WebManager.Services
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

        public Student Get(string GUID)
        {
            return _repository.GetById(GUID);
        }

        public IEnumerable<Student> GetAll()
        {
            return _repository.GetAll();
        }

        public IEnumerable<Student> GetAllActive()
        {
            return _repository.GetAll().Where(x => x.IsActive == true);
        }

        public IEnumerable<Student> GetAllForSchool(School school)
        {
            return _repository.GetAll().Where(x => x.IsActive == true);
        }

        public void InsertOrUpdate(Student Student)
        {
            _repository.Update(Student);
        }

        public void Delete(Student student) {
            _repository.Delete(student);
        }

    }

}
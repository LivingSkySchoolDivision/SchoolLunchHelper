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
    public class StudentsRepository
    {
        private readonly DataDbContext _context;

        public StudentsRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<Student> FindAsync(string id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void ModifiedEntityState(Student student)
        {
            _context.Entry(student).State = EntityState.Modified;
        }

        public void Add(Student student)
        {
            _context.Students.Add(student);
        }

        public EntityEntry<Student> Remove(Student student)
        {
            return _context.Students.Remove(student);
        }

        public bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentID == id);
        }

    }
}

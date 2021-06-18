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
    public class SchoolsRepository
    {
        private readonly DataDbContext _context = ContextInjector.Context;

        public SchoolsRepository()
        {
        }

        public async Task<ActionResult<IEnumerable<School>>> GetSchools()
        {
            return await _context.Schools.ToListAsync();
        }

        public async Task<School> FindAsync(string id)
        {
            return await _context.Schools.FindAsync(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void ModifiedEntityState(School school)
        {
            _context.Entry(school).State = EntityState.Modified;
        }

        public void Add(School school)
        {
            _context.Schools.Add(school);
        }

        public EntityEntry<School> Remove(School school)
        {
            return _context.Schools.Remove(school);
        }

        public bool SchoolExists(string id)
        {
            return _context.Schools.Any(e => e.ID == id);
        }
    }
}

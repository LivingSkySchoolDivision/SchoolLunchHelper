﻿using System;
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
    /**<summary>A wrapper class to keep EF Core separate from other classes. Handles the school objects.</summary>
     */
    public class SchoolsRepository
    {
        //private readonly DataDbContext _context = ContextInjector.Context;
        private DataDbContext _context;

        public SchoolsRepository()
        {
            _context = DbContextManager.GetNewDbContext();
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
            var saveChanges = await _context.SaveChangesAsync();
            _context.Dispose();
            _context = DbContextManager.GetNewDbContext();
            return saveChanges;
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

        public async Task UpdateSchool(School newSchool)
        {
            School oldSchool = await _context.Schools.FindAsync(newSchool.ID);
            oldSchool.Name = newSchool.Name;
            _context.Update(oldSchool);

        }
    }
}

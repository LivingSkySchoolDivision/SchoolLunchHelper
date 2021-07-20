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
    /**<summary>A wrapper class to keep EF Core separate from other classes. Handles the student objects.</summary>
     */
    public class StudentsRepository
    {
        //private readonly DataDbContext _context = ContextInjector.Context;
        private DataDbContext _context;

        public StudentsRepository()
        {
            _context = DbContextManager.GetNewDbContext();
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
            var saveChanges = await _context.SaveChangesAsync();
            _context.Dispose();
            _context = DbContextManager.GetNewDbContext();
            return saveChanges;
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

        public async Task UpdateStudentInfo(Student newStudentInfo)
        {
            //find the entity, then change its fields, then update it
            Student oldStudentInfo = await _context.Students.FindAsync(newStudentInfo.StudentID);
            oldStudentInfo.Balance = newStudentInfo.Balance;
            oldStudentInfo.MedicalInfo = newStudentInfo.MedicalInfo;
            oldStudentInfo.Name = newStudentInfo.Name;
            oldStudentInfo.SchoolID = newStudentInfo.SchoolID;
            _context.Update(oldStudentInfo);
        }

    }
}

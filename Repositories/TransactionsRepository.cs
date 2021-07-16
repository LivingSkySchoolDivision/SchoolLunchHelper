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
    /**<summary>Separates EF Core code from the Transaction controller</summary>
     */
    public class TransactionsRepository
    {
        //private readonly DataDbContext _context = ContextInjector.Context;
        private DataDbContext _context;

        public TransactionsRepository()
        {
            _context = DbContextManager.GetNewDbContext();
        }

        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task<Transaction> FindAsync(string id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            var saveChanges = await _context.SaveChangesAsync();
            _context.Dispose();
            _context = DbContextManager.GetNewDbContext();
            return saveChanges;
        }

        /*
        public void ModifiedEntityState(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
        }
        */

        public void Add(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
        }

        public EntityEntry<Transaction> Remove(Transaction transaction)
        {
            return _context.Transactions.Remove(transaction);
        }

        public bool TransactionExists(string id)
        {
            return _context.Transactions.Any(e => e.ID == id);
        }

        public async Task UpdateTransaction(Transaction newTransaction)
        {
            //find the entity, then change its fields, then update it
            Transaction oldTransaction = await _context.Transactions.FindAsync(newTransaction.ID);
            oldTransaction.Cost = newTransaction.Cost;
            oldTransaction.FoodID = newTransaction.FoodID;
            oldTransaction.FoodName = newTransaction.FoodName;
            oldTransaction.SchoolID = newTransaction.SchoolID;
            oldTransaction.SchoolName = newTransaction.SchoolName;
            oldTransaction.StudentID = newTransaction.StudentID;
            oldTransaction.StudentName = newTransaction.StudentName;
            oldTransaction.Time = newTransaction.Time;
            _context.Update(oldTransaction);
        }

    }
}

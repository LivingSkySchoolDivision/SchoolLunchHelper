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
        private readonly DataDbContext _context;

        public TransactionsRepository(DataDbContext context)
        {
            _context = context;
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
            return await _context.SaveChangesAsync();
        }

        public void ModifiedEntityState(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
        }

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
    }
}

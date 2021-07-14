using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Models;
using lunch_project.Classes;
using Repositories;
using System.Diagnostics; //DEBUG 

namespace LunchAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionsRepository repo;

        private readonly BalanceCalculator balanceCalculator;

        public TransactionsController()
        {
            repo = new TransactionsRepository();
            balanceCalculator = new BalanceCalculator();
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await repo.GetTransactions();
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(string id)
        {
            var transaction = await repo.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // GET: api/Transactions/School/5
        [HttpGet("School/{schoolID}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFoodItemsBySchool(string schoolID)
        {
            var allTransactions = await repo.GetTransactions();
            //this is slower than directly querying EF Core, but it is less likely to cause an exception since it does not involve EF Core translating the LINQ statement to SQL
            var requestedTransactions = allTransactions.Value.Where(p => string.Equals(p.SchoolID, schoolID, StringComparison.OrdinalIgnoreCase));
            return new ActionResult<IEnumerable<Transaction>>(requestedTransactions);
        }

        // PUT: api/Transactions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // Modifies existing transactions. Assumes fields related to the field that was modified were updated before the put request was made.
        // Ex. if the food ID changes, this method assumes the food name was updated as well
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(string id, Transaction transaction)
        {
            if (id != transaction.ID)
            {
                return BadRequest();
            }

            //finding the cost of the transaction in the database before it is modified so the student's balance can be changed accordingly
            Transaction repoTransaction = await repo.FindAsync(id);
            if (repoTransaction == null)
            {
                return NotFound();
            }
            else if (repoTransaction.StudentID != transaction.StudentID) //if the transaction is changed to belong to a different student
            {//NOTE: in this case the controller assumes the student's name and any other fields that need to be changed were changed before the put request was made
                await balanceCalculator.UpdateBalanceRemoveTransaction(repoTransaction); //the transaction is reversed on the old student's balance
                await balanceCalculator.UpdateBalanceNewTransaction(transaction); //the transaction is added to the new student's balance
            }
            else if (repoTransaction.Cost != transaction.Cost) //if the old and new cost are not the same, the student's balance needs to be updated
            {
                await balanceCalculator.UpdateBalanceModifiedTransactionCost(transaction, repoTransaction.Cost);
            }

            repo.ModifiedEntityState(transaction);

            try
            {
                await repo.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!repo.TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Transactions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // Adds new transactions, if the transaction exists, it will not be added
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            Trace.WriteLine("PostTransaction->  name: " + transaction.StudentName + " ID: " + transaction.StudentID + " cost: " + transaction.Cost + " item: " + transaction.FoodName + " foodID: " + transaction.FoodID + " schoolName: " + transaction.SchoolName + " schoolID: " + transaction.SchoolID); //DEBUG

            repo.Add(transaction);
            try
            {
                await repo.SaveChangesAsync();
                await balanceCalculator.UpdateBalanceNewTransaction(transaction);
            }
            catch (DbUpdateException)
            {
                if (repo.TransactionExists(transaction.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            //return Conflict(); //DEBUG - testing error handling
            return CreatedAtAction("GetTransaction", new { id = transaction.ID }, transaction);
        }

        // DELETE: api/Transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var transaction = await repo.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            await balanceCalculator.UpdateBalanceRemoveTransaction(transaction);

            repo.Remove(transaction);
            await repo.SaveChangesAsync();

            return NoContent();
        }

        
    }
}

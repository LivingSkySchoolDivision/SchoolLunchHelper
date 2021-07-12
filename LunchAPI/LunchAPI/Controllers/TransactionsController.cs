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

        // PUT: api/Transactions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // Modifies existing transactions 
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
            {//if the student ID changes, the student name (and in some cases school ID and name) in the transaction will also need to change (probably better to handle in the program itself)
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

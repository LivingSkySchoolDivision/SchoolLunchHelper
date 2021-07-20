using lunch_project.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    /**<summary>Updates student balances when a transaction is created, updated, or deleted</summary>
     */
    public class BalanceCalculator 
    {
        private readonly StudentsRepository studentsRepo = new();

        public BalanceCalculator()
        {

        }


        /**<summary>Updates a student's balance with the cost of the given transaction.</summary>
         * <param name="transaction">The transaction to be applied to a student's balance.</param>
         */
        public async Task UpdateBalanceNewTransaction(Transaction transaction)
        { 
            Student student = await studentsRepo.FindAsync(transaction.StudentID); //find the student that the transaction belongs to
            student.Balance -= transaction.Cost; //subtract the cost from the student's balance
            studentsRepo.ModifiedEntityState(student); //tell EF Core that the student has been modified
            await studentsRepo.SaveChangesAsync(); //save changes to the database
        }

        /**<summary>Refunds a transaction from the student's balance.</summary>
         * <param name="transaction">The transaction to be undone.</param>
         */
        public async Task UpdateBalanceRemoveTransaction(Transaction transaction)
        {
            Student student = await studentsRepo.FindAsync(transaction.StudentID);
            if (student != null)
            {
                student.Balance += transaction.Cost;
                studentsRepo.ModifiedEntityState(student);
                await studentsRepo.SaveChangesAsync();
            }
        }

        /**<summary>Adjusts the student's balance when a transaction changes its cost.</summary>
         * <param name="updatedTransaction">The updated transaction.</param>
         * <param name="oldCost">The original cost of the transaction.</param>
         */
        public async Task UpdateBalanceModifiedTransactionCost(Transaction updatedTransaction, Decimal oldCost)
        { //this needs to return before the transaction is modified in EF Core
            Student student = await studentsRepo.FindAsync(updatedTransaction.StudentID);

            //refund the cost of the old transaction and subtract the cost of the new transaction from the student's balance
            student.Balance += oldCost;
            student.Balance -= updatedTransaction.Cost;

            studentsRepo.ModifiedEntityState(student);
            await studentsRepo.SaveChangesAsync();
        }

        /**<summary>Checks if a student with a certain ID exists.</summary>
         * <param name="studentID">The ID of the student to search for.</param>
         * <returns>True if the student exists, false if the student does not exist.</returns>
         */
        public async Task<bool> StudentWithIdExists(string studentID)
        {
            Student student = await studentsRepo.FindAsync(studentID);
            if (student != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}

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


        public async Task UpdateBalanceNewTransaction(Transaction transaction)
        { //exceptions are handled by the transactions controller
            Student student = await studentsRepo.FindAsync(transaction.StudentID); //find the student that the transaction belongs to
            student.Balance -= transaction.Cost; //subtract the cost from the student's balance
            studentsRepo.ModifiedEntityState(student); //tell EF Core that the student has been modified
            await studentsRepo.SaveChangesAsync();
        }


        public async Task UpdateBalanceRemoveTransaction(Transaction transaction)
        {
            Student student = await studentsRepo.FindAsync(transaction.StudentID);
            student.Balance += transaction.Cost;
            studentsRepo.ModifiedEntityState(student);
            await studentsRepo.SaveChangesAsync();
        }


        public async Task UpdateBalanceModifiedTransactionCost(Transaction updatedTransaction, Decimal oldCost)
        { //this needs to return before the transaction is modified in EF Core
            Student student = await studentsRepo.FindAsync(updatedTransaction.StudentID);

            //refund the cost of the old transaction and subtract the cost of the new transaction from the student's balance
            student.Balance += oldCost;
            student.Balance -= updatedTransaction.Cost;

            studentsRepo.ModifiedEntityState(student);
            await studentsRepo.SaveChangesAsync();
        }


    }
}

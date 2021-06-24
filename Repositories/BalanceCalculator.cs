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
    public class BalanceCalculator 
    {
        private readonly StudentsRepository repo = new();


        public BalanceCalculator()
        {

        }


        public async Task<bool> UpdateBalanceNewTransaction(Transaction transaction) //may need to change return type
        {
            var student = await repo.FindAsync(transaction.StudentID); //need to check that student can be found since this doesn't get called from the student controller
            if (student == null)
            {
                return false;
            }

            student.Balance -= transaction.Cost;

            repo.ModifiedEntityState(student);
            try
            {
                await repo.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }


        public async Task<bool> UpdateBalanceModifiedTransaction(Transaction transaction, Decimal oldCost) //may need to change return type
        { //need the old transaction's cost as an argument
            var student = await repo.FindAsync(transaction.StudentID); //need to check that student can be found since this doesn't get called from the student controller
            if (student == null)
            {
                return false;
            }

            student.Balance += oldCost;
            student.Balance -= transaction.Cost;

            repo.ModifiedEntityState(student);
            try
            {
                await repo.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateBalanceDeleteTransaction(Transaction transaction)
        {//if this returns false the transaction should not be deleted, this method needs to be called/return before deleting the transaction

        }

    }
}

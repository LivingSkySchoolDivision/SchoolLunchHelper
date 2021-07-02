using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lunch_project.Classes;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public static class ContextInjector 
    {
        //private static DataDbContext context = new DataDbContext(); 
        private static DataDbContext context;
        public static DataDbContext Context { get { return context; } }
        //private static string connectionString;


        public static void Init(string connectionString)
        {
            context = new DataDbContext(connectionString);
            //ContextInjector.connectionString = connectionString;
        }

        public static void Init(DataDbContext context)
        {
            ContextInjector.context = context;
        }



        /*
        private static async DataDbContext GetDataContext()
        {
            return await 
        }
        */

    }
}

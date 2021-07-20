using lunch_project.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    /** Creates instances of DbContext classes 
     */
    public static class DbContextManager
    {
        private static string ConnectionString;


        public static void Init(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static DataDbContext GetNewDbContext()
        {
            return new DataDbContext(ConnectionString);
        }

    }
}

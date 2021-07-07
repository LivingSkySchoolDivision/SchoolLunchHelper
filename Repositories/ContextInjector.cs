﻿using System;
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
        private static DataDbContext context;
        public static DataDbContext Context { get { return context; } }


        public static void Init(string connectionString)
        {
            context = new DataDbContext(connectionString);
        }

        /*
        public static void Init(DataDbContext context)
        {
            ContextInjector.context = context;
        }
        */

    }
}

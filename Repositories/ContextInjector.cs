﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lunch_project.Classes;

namespace Repositories
{
    public static class ContextInjector
    {
        private static DataDbContext context = new DataDbContext();
        public static DataDbContext Context { get; }
    }
}

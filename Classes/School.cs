using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lunch_project
{
    public class School
    {
        public string Name { get; }
        public int ID { get; }

        public School(string Name, int ID)
        {
            this.Name = Name;
            this.ID = ID;
        }
    }
}

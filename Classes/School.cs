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
        public string ID { get; }

        public School(string Name, string ID)
        {
            this.Name = Name;
            this.ID = ID;
        }
    }
}

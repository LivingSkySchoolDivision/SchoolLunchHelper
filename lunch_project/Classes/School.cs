using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lunch_project
{
    public class School
    {
        private string _Name;
        private string _ID;

        //private setters are for EF Core
        public string Name { get { return _Name; } private set { _Name = value; } }
        public string ID { get { return _ID; } private set { _ID = value; } }

        public School(string Name, string ID)
        {
            _Name = Name;
            _ID = ID;
        }
    }
}

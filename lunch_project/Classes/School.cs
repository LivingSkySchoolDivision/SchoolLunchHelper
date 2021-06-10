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

        public string Name { get { return _Name; } }
        public string ID { get { return _ID; } }

        public School(string Name, string ID)
        {
            _Name = Name;
            _ID = ID;
        }
    }
}

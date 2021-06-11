using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class School
    {
        /*
        [Required]
        public string Name { get; set; }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        */

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

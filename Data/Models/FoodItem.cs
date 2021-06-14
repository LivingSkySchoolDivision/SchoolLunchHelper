using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class FoodItem
    {
        private string _Name;
        private string _ID;
        private string _SchoolID;
        private double _Cost;
        private string _Description;

        //private setters are for EF Core
        [Required]
        public string Name { get { return _Name; } private set { _Name = value; } }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get { return _ID; } private set { _ID = value; } }

        [Required]
        public string SchoolID { get { return _SchoolID; } private set { _SchoolID = value; } }

        [Column(TypeName = "decimal(18, 2)"), Required]
        public double Cost { get { return _Cost; } private set { _Cost = value; } }

        [Required(AllowEmptyStrings = true)]
        public string Description { get { return _Description; } private set { _Description = value; } }


        private FoodItem(string Name, string SchoolID, string ID, double Cost, string Description)
        {
            _Name = Name;
            _SchoolID = SchoolID;
            _ID = ID;
            _Cost = Cost;
            _Description = Description;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Transaction
    {
        /*
        [Column(TypeName = "decimal(18, 2)"), Required]
        public double Cost { get; set; }

        [Required]
        public string StudentID { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Required]
        public string FoodID { get; set; }

        [Required]
        public string FoodName { get; set; }

        [Required]
        public string SchoolID { get; set; }

        [Required]
        public string SchoolName { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
        */

        private double _Cost;
        private string _StudentID;
        private string _StudentName;
        private string _FoodID;
        private string _FoodName;
        private string _SchoolID;
        private string _SchoolName;
        private DateTime _Time;
        private string _ID;

        //private setters are for EF Core
        public double Cost { get { return _Cost; } private set { _Cost = value; } }
        public string StudentID { get { return _StudentID; } private set { _StudentID = value; } }
        public string StudentName { get { return _StudentName; } private set { _StudentName = value; } }
        public string FoodID { get { return _FoodID; } private set { _FoodID = value; } }
        public string FoodName { get { return _FoodName; } private set { _FoodName = value; } }
        public string SchoolID { get { return _SchoolID; } private set { _SchoolID = value; } }
        public string SchoolName { get { return _SchoolName; } private set { _SchoolName = value; } }
        public DateTime Time { get { return _Time; } private set { _Time = value; } }
        public string ID { get { return _ID; } private set { _ID = value; } }

        private Transaction(double Cost, string StudentID, string StudentName, string FoodID, string FoodName, string SchoolID, string SchoolName, DateTime Time, string ID)
        {
            _Cost = Cost;
            _StudentID = StudentID;
            _StudentName = StudentName;
            _FoodID = FoodID;
            _FoodName = FoodName;
            _SchoolID = SchoolID;
            _SchoolName = SchoolName;
            _Time = Time;
            _ID = ID;
        }
    }
}
